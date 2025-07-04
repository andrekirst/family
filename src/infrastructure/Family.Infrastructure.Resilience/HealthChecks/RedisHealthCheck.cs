using StackExchange.Redis;

namespace Family.Infrastructure.Resilience.HealthChecks;

/// <summary>
/// Health check for Redis connectivity and functionality
/// </summary>
public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger<RedisHealthCheck> _logger;

    public RedisHealthCheck(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisHealthCheck> logger)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            
            if (!_connectionMultiplexer.IsConnected)
            {
                return HealthCheckResult.Unhealthy(
                    "Redis connection is not established",
                    data: new Dictionary<string, object>
                    {
                        ["connection_state"] = "Disconnected",
                        ["duration"] = stopwatch.ElapsedMilliseconds
                    });
            }

            var database = _connectionMultiplexer.GetDatabase();
            
            // Test Redis with a simple ping
            var pingResult = await database.PingAsync();
            
            // Test basic set/get operations
            var testKey = $"health_check_{Guid.NewGuid()}";
            var testValue = "health_check_value";
            
            await database.StringSetAsync(testKey, testValue, TimeSpan.FromSeconds(10));
            var retrievedValue = await database.StringGetAsync(testKey);
            await database.KeyDeleteAsync(testKey);
            
            stopwatch.Stop();

            if (retrievedValue != testValue)
            {
                return HealthCheckResult.Degraded(
                    "Redis set/get operations not working correctly",
                    data: new Dictionary<string, object>
                    {
                        ["ping_ms"] = pingResult.TotalMilliseconds,
                        ["duration"] = stopwatch.ElapsedMilliseconds,
                        ["connection_state"] = "Connected"
                    });
            }

            var data = new Dictionary<string, object>
            {
                ["ping_ms"] = pingResult.TotalMilliseconds,
                ["duration"] = stopwatch.ElapsedMilliseconds,
                ["connection_state"] = "Connected",
                ["endpoints"] = string.Join(", ", _connectionMultiplexer.GetEndPoints().Select(ep => ep.ToString()))
            };

            return HealthCheckResult.Healthy(
                "Redis is responsive",
                data: data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed");
            
            return HealthCheckResult.Unhealthy(
                "Redis health check failed",
                ex,
                data: new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["connection_state"] = _connectionMultiplexer.IsConnected ? "Connected" : "Disconnected"
                });
        }
    }
}