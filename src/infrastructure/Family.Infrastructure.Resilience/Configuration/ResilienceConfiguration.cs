namespace Family.Infrastructure.Resilience.Configuration;

/// <summary>
/// Configuration for resilience patterns
/// </summary>
public class ResilienceConfiguration
{
    public const string SectionName = "Resilience";

    /// <summary>
    /// Health check configuration
    /// </summary>
    public HealthCheckConfiguration HealthChecks { get; set; } = new();

    /// <summary>
    /// Circuit breaker configuration
    /// </summary>
    public CircuitBreakerConfiguration CircuitBreaker { get; set; } = new();

    /// <summary>
    /// Retry policy configuration
    /// </summary>
    public RetryConfiguration Retry { get; set; } = new();

    /// <summary>
    /// Timeout configuration
    /// </summary>
    public TimeoutConfiguration Timeout { get; set; } = new();
}

/// <summary>
/// Health check configuration options
/// </summary>
public class HealthCheckConfiguration
{
    /// <summary>
    /// Health check endpoint path
    /// </summary>
    public string HealthPath { get; set; } = "/health";

    /// <summary>
    /// Ready endpoint path for readiness probes
    /// </summary>
    public string ReadyPath { get; set; } = "/health/ready";

    /// <summary>
    /// Live endpoint path for liveness probes
    /// </summary>
    public string LivePath { get; set; } = "/health/live";

    /// <summary>
    /// Enable health checks UI
    /// </summary>
    public bool EnableUI { get; set; } = true;

    /// <summary>
    /// Health checks UI path
    /// </summary>
    public string UIPath { get; set; } = "/health-ui";

    /// <summary>
    /// Health check evaluation interval
    /// </summary>
    public TimeSpan EvaluationInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Health check timeout
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
}

/// <summary>
/// Circuit breaker configuration
/// </summary>
public class CircuitBreakerConfiguration
{
    /// <summary>
    /// Number of consecutive failures before opening circuit
    /// </summary>
    public int HandledEventsAllowedBeforeBreaking { get; set; } = 5;

    /// <summary>
    /// Duration of break period
    /// </summary>
    public TimeSpan DurationOfBreak { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Minimum throughput for circuit breaker evaluation
    /// </summary>
    public int MinimumThroughput { get; set; } = 10;

    /// <summary>
    /// Sampling duration for circuit breaker evaluation
    /// </summary>
    public TimeSpan SamplingDuration { get; set; } = TimeSpan.FromSeconds(60);
}

/// <summary>
/// Retry policy configuration
/// </summary>
public class RetryConfiguration
{
    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Base delay between retries
    /// </summary>
    public TimeSpan BaseDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Maximum delay between retries
    /// </summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Enable jitter to prevent thundering herd
    /// </summary>
    public bool UseJitter { get; set; } = true;
}

/// <summary>
/// Timeout configuration
/// </summary>
public class TimeoutConfiguration
{
    /// <summary>
    /// Default timeout for operations
    /// </summary>
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Database operation timeout
    /// </summary>
    public TimeSpan DatabaseTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// External API timeout
    /// </summary>
    public TimeSpan ExternalApiTimeout { get; set; } = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Cache operation timeout
    /// </summary>
    public TimeSpan CacheTimeout { get; set; } = TimeSpan.FromSeconds(5);
}