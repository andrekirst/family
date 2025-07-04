using Family.Infrastructure.Resilience.Abstractions;
using Family.Infrastructure.Resilience.Configuration;
using Polly.Registry;

namespace Family.Infrastructure.Resilience.Services;

/// <summary>
/// Service for executing operations with resilience patterns
/// </summary>
public class ResilienceService : IResilienceService
{
    private readonly ResiliencePipelineProvider<string> _pipelineProvider;
    private readonly ILogger<ResilienceService> _logger;

    public ResilienceService(
        ResiliencePipelineProvider<string> pipelineProvider,
        ILogger<ResilienceService> logger)
    {
        _pipelineProvider = pipelineProvider;
        _logger = logger;
    }

    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        string policyKey = "default",
        CancellationToken cancellationToken = default)
    {
        var pipeline = GetPipeline<T>(policyKey);
        
        return await pipeline.ExecuteAsync(async (ct) =>
        {
            _logger.LogDebug("Executing operation with resilience policy '{PolicyKey}'", policyKey);
            return await operation(ct);
        }, cancellationToken);
    }

    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        string policyKey = "default",
        CancellationToken cancellationToken = default)
    {
        var pipeline = GetPipeline(policyKey);
        
        await pipeline.ExecuteAsync(async (ct) =>
        {
            _logger.LogDebug("Executing operation with resilience policy '{PolicyKey}'", policyKey);
            await operation(ct);
        }, cancellationToken);
    }

    public ResiliencePipeline GetPipeline(string policyKey = "default")
    {
        return _pipelineProvider.GetPipeline(policyKey);
    }

    public ResiliencePipeline<T> GetPipeline<T>(string policyKey = "default")
    {
        return _pipelineProvider.GetPipeline<T>(policyKey);
    }
}

/// <summary>
/// Factory for creating resilience pipelines
/// </summary>
public class ResiliencePipelineFactory : IResiliencePipelineFactory
{
    private readonly IOptions<ResilienceConfiguration> _options;
    private readonly ILogger<ResiliencePipelineFactory> _logger;

    public ResiliencePipelineFactory(
        IOptions<ResilienceConfiguration> options,
        ILogger<ResiliencePipelineFactory> logger)
    {
        _options = options;
        _logger = logger;
    }

    public ResiliencePipeline CreateDatabasePipeline()
    {
        var config = _options.Value;
        
        return new ResiliencePipelineBuilder()
            .AddTimeout(config.Timeout.DatabaseTimeout)
            .AddRetry(new Polly.Retry.RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                MaxRetryAttempts = config.Retry.MaxRetryAttempts,
                Delay = config.Retry.BaseDelay,
                MaxDelay = config.Retry.MaxDelay,
                UseJitter = config.Retry.UseJitter,
                OnRetry = args =>
                {
                    _logger.LogWarning("Database operation retry attempt {AttemptNumber} after {Delay}ms. Exception: {Exception}",
                        args.AttemptNumber, args.RetryDelay.TotalMilliseconds, args.Outcome.Exception?.Message);
                    return ValueTask.CompletedTask;
                }
            })
            .AddCircuitBreaker(new Polly.CircuitBreaker.CircuitBreakerStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                FailureRatio = 0.5,
                MinimumThroughput = config.CircuitBreaker.MinimumThroughput,
                SamplingDuration = config.CircuitBreaker.SamplingDuration,
                BreakDuration = config.CircuitBreaker.DurationOfBreak,
                OnOpened = args =>
                {
                    _logger.LogWarning("Database circuit breaker opened. Break duration: {BreakDuration}ms",
                        args.BreakDuration.TotalMilliseconds);
                    return ValueTask.CompletedTask;
                },
                OnClosed = args =>
                {
                    _logger.LogInformation("Database circuit breaker closed");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    public ResiliencePipeline CreateExternalApiPipeline()
    {
        var config = _options.Value;
        
        return new ResiliencePipelineBuilder()
            .AddTimeout(config.Timeout.ExternalApiTimeout)
            .AddRetry(new Polly.Retry.RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<HttpRequestException>()
                    .Handle<TaskCanceledException>(),
                MaxRetryAttempts = config.Retry.MaxRetryAttempts,
                Delay = config.Retry.BaseDelay,
                MaxDelay = config.Retry.MaxDelay,
                UseJitter = config.Retry.UseJitter,
                OnRetry = args =>
                {
                    _logger.LogWarning("External API retry attempt {AttemptNumber} after {Delay}ms. Exception: {Exception}",
                        args.AttemptNumber, args.RetryDelay.TotalMilliseconds, args.Outcome.Exception?.Message);
                    return ValueTask.CompletedTask;
                }
            })
            .AddCircuitBreaker(new Polly.CircuitBreaker.CircuitBreakerStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<HttpRequestException>()
                    .Handle<TaskCanceledException>(),
                FailureRatio = 0.6,
                MinimumThroughput = config.CircuitBreaker.MinimumThroughput,
                SamplingDuration = config.CircuitBreaker.SamplingDuration,
                BreakDuration = config.CircuitBreaker.DurationOfBreak,
                OnOpened = args =>
                {
                    _logger.LogWarning("External API circuit breaker opened. Break duration: {BreakDuration}ms",
                        args.BreakDuration.TotalMilliseconds);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    public ResiliencePipeline CreateCachePipeline()
    {
        var config = _options.Value;
        
        return new ResiliencePipelineBuilder()
            .AddTimeout(config.Timeout.CacheTimeout)
            .AddRetry(new Polly.Retry.RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                MaxRetryAttempts = 2, // Fewer retries for cache operations
                Delay = TimeSpan.FromMilliseconds(100),
                MaxDelay = TimeSpan.FromSeconds(1),
                UseJitter = true,
                OnRetry = args =>
                {
                    _logger.LogDebug("Cache operation retry attempt {AttemptNumber}. Exception: {Exception}",
                        args.AttemptNumber, args.Outcome.Exception?.Message);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    public ResiliencePipeline<T> CreateTypedPipeline<T>(ResiliencePipelineType pipelineType)
    {
        return pipelineType switch
        {
            ResiliencePipelineType.Database => CreateTypedDatabasePipeline<T>(),
            ResiliencePipelineType.ExternalApi => CreateTypedExternalApiPipeline<T>(),
            ResiliencePipelineType.Cache => CreateTypedCachePipeline<T>(),
            _ => CreateDefaultPipeline<T>()
        };
    }

    private ResiliencePipeline<T> CreateTypedDatabasePipeline<T>()
    {
        var config = _options.Value;
        
        return new ResiliencePipelineBuilder<T>()
            .AddTimeout(config.Timeout.DatabaseTimeout)
            .AddRetry(new Polly.Retry.RetryStrategyOptions<T>
            {
                ShouldHandle = new PredicateBuilder<T>().Handle<Exception>(),
                MaxRetryAttempts = config.Retry.MaxRetryAttempts,
                Delay = config.Retry.BaseDelay,
                MaxDelay = config.Retry.MaxDelay,
                UseJitter = config.Retry.UseJitter
            })
            .AddCircuitBreaker(new Polly.CircuitBreaker.CircuitBreakerStrategyOptions<T>
            {
                ShouldHandle = new PredicateBuilder<T>().Handle<Exception>(),
                FailureRatio = 0.5,
                MinimumThroughput = config.CircuitBreaker.MinimumThroughput,
                SamplingDuration = config.CircuitBreaker.SamplingDuration,
                BreakDuration = config.CircuitBreaker.DurationOfBreak
            })
            .Build();
    }

    private ResiliencePipeline<T> CreateTypedExternalApiPipeline<T>()
    {
        var config = _options.Value;
        
        return new ResiliencePipelineBuilder<T>()
            .AddTimeout(config.Timeout.ExternalApiTimeout)
            .AddRetry(new Polly.Retry.RetryStrategyOptions<T>
            {
                ShouldHandle = new PredicateBuilder<T>().Handle<Exception>(),
                MaxRetryAttempts = config.Retry.MaxRetryAttempts,
                Delay = config.Retry.BaseDelay,
                MaxDelay = config.Retry.MaxDelay,
                UseJitter = config.Retry.UseJitter
            })
            .AddCircuitBreaker(new Polly.CircuitBreaker.CircuitBreakerStrategyOptions<T>
            {
                ShouldHandle = new PredicateBuilder<T>().Handle<Exception>(),
                FailureRatio = 0.6,
                MinimumThroughput = config.CircuitBreaker.MinimumThroughput,
                SamplingDuration = config.CircuitBreaker.SamplingDuration,
                BreakDuration = config.CircuitBreaker.DurationOfBreak
            })
            .Build();
    }

    private ResiliencePipeline<T> CreateTypedCachePipeline<T>()
    {
        var config = _options.Value;
        
        return new ResiliencePipelineBuilder<T>()
            .AddTimeout(config.Timeout.CacheTimeout)
            .AddRetry(new Polly.Retry.RetryStrategyOptions<T>
            {
                ShouldHandle = new PredicateBuilder<T>().Handle<Exception>(),
                MaxRetryAttempts = 2,
                Delay = TimeSpan.FromMilliseconds(100),
                MaxDelay = TimeSpan.FromSeconds(1),
                UseJitter = true
            })
            .Build();
    }

    private ResiliencePipeline<T> CreateDefaultPipeline<T>()
    {
        var config = _options.Value;
        
        return new ResiliencePipelineBuilder<T>()
            .AddTimeout(config.Timeout.DefaultTimeout)
            .AddRetry(new Polly.Retry.RetryStrategyOptions<T>
            {
                ShouldHandle = new PredicateBuilder<T>().Handle<Exception>(),
                MaxRetryAttempts = config.Retry.MaxRetryAttempts,
                Delay = config.Retry.BaseDelay,
                MaxDelay = config.Retry.MaxDelay,
                UseJitter = config.Retry.UseJitter
            })
            .Build();
    }
}