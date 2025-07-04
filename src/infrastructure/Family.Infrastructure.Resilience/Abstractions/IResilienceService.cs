namespace Family.Infrastructure.Resilience.Abstractions;

/// <summary>
/// Service for executing operations with resilience patterns
/// </summary>
public interface IResilienceService
{
    /// <summary>
    /// Executes an async operation with retry and circuit breaker patterns
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <param name="policyKey">Policy key for resilience strategy</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the operation</returns>
    Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        string policyKey = "default",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an async operation with resilience patterns
    /// </summary>
    /// <param name="operation">Operation to execute</param>
    /// <param name="policyKey">Policy key for resilience strategy</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        string policyKey = "default",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets resilience pipeline for a specific key
    /// </summary>
    /// <param name="policyKey">Policy key</param>
    /// <returns>Resilience pipeline</returns>
    ResiliencePipeline GetPipeline(string policyKey = "default");

    /// <summary>
    /// Gets typed resilience pipeline for a specific key
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="policyKey">Policy key</param>
    /// <returns>Typed resilience pipeline</returns>
    ResiliencePipeline<T> GetPipeline<T>(string policyKey = "default");
}

/// <summary>
/// Factory for creating resilience pipelines
/// </summary>
public interface IResiliencePipelineFactory
{
    /// <summary>
    /// Creates a resilience pipeline for database operations
    /// </summary>
    /// <returns>Database resilience pipeline</returns>
    ResiliencePipeline CreateDatabasePipeline();

    /// <summary>
    /// Creates a resilience pipeline for external API calls
    /// </summary>
    /// <returns>External API resilience pipeline</returns>
    ResiliencePipeline CreateExternalApiPipeline();

    /// <summary>
    /// Creates a resilience pipeline for cache operations
    /// </summary>
    /// <returns>Cache resilience pipeline</returns>
    ResiliencePipeline CreateCachePipeline();

    /// <summary>
    /// Creates a typed resilience pipeline for specific operations
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="pipelineType">Type of pipeline</param>
    /// <returns>Typed resilience pipeline</returns>
    ResiliencePipeline<T> CreateTypedPipeline<T>(ResiliencePipelineType pipelineType);
}

/// <summary>
/// Types of resilience pipelines
/// </summary>
public enum ResiliencePipelineType
{
    Default,
    Database,
    ExternalApi,
    Cache,
    MessageQueue
}