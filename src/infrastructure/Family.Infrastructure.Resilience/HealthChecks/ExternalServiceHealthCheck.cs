namespace Family.Infrastructure.Resilience.HealthChecks;

/// <summary>
/// Health check for external services and APIs
/// </summary>
public class ExternalServiceHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalServiceHealthCheck> _logger;
    private readonly string _serviceName;
    private readonly string _healthEndpoint;
    private readonly TimeSpan _timeout;

    public ExternalServiceHealthCheck(
        HttpClient httpClient,
        ILogger<ExternalServiceHealthCheck> logger,
        string serviceName,
        string healthEndpoint,
        TimeSpan? timeout = null)
    {
        _httpClient = httpClient;
        _logger = logger;
        _serviceName = serviceName;
        _healthEndpoint = healthEndpoint;
        _timeout = timeout ?? TimeSpan.FromSeconds(10);
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(_timeout);

            var response = await _httpClient.GetAsync(_healthEndpoint, cts.Token);
            stopwatch.Stop();

            var data = new Dictionary<string, object>
            {
                ["service"] = _serviceName,
                ["endpoint"] = _healthEndpoint,
                ["duration"] = stopwatch.ElapsedMilliseconds,
                ["status_code"] = (int)response.StatusCode
            };

            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy(
                    $"External service '{_serviceName}' is responsive",
                    data: data);
            }
            else
            {
                data["response_body"] = await response.Content.ReadAsStringAsync(cancellationToken);
                
                return HealthCheckResult.Unhealthy(
                    $"External service '{_serviceName}' returned status code {response.StatusCode}",
                    data: data);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy(
                $"External service '{_serviceName}' health check was cancelled",
                data: new Dictionary<string, object>
                {
                    ["service"] = _serviceName,
                    ["endpoint"] = _healthEndpoint,
                    ["error"] = "Operation was cancelled"
                });
        }
        catch (TaskCanceledException)
        {
            return HealthCheckResult.Unhealthy(
                $"External service '{_serviceName}' health check timed out",
                data: new Dictionary<string, object>
                {
                    ["service"] = _serviceName,
                    ["endpoint"] = _healthEndpoint,
                    ["timeout_ms"] = _timeout.TotalMilliseconds,
                    ["error"] = "Timeout"
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "External service '{ServiceName}' health check failed", _serviceName);
            
            return HealthCheckResult.Unhealthy(
                $"External service '{_serviceName}' health check failed",
                ex,
                data: new Dictionary<string, object>
                {
                    ["service"] = _serviceName,
                    ["endpoint"] = _healthEndpoint,
                    ["error"] = ex.Message
                });
        }
    }
}