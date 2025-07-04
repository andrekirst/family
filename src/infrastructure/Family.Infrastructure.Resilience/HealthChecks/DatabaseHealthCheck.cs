using Microsoft.EntityFrameworkCore;

namespace Family.Infrastructure.Resilience.HealthChecks;

/// <summary>
/// Health check for database connectivity and functionality
/// </summary>
public class DatabaseHealthCheck<TDbContext> : IHealthCheck
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private readonly ILogger<DatabaseHealthCheck<TDbContext>> _logger;

    public DatabaseHealthCheck(TDbContext dbContext, ILogger<DatabaseHealthCheck<TDbContext>> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();

            // Test database connectivity
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            
            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy(
                    "Database connection failed",
                    data: new Dictionary<string, object>
                    {
                        ["database"] = _dbContext.Database.GetConnectionString() ?? "Unknown",
                        ["duration"] = stopwatch.ElapsedMilliseconds
                    });
            }

            // Test a simple query to ensure database is responsive
            var result = await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT 1", cancellationToken);

            stopwatch.Stop();

            var data = new Dictionary<string, object>
            {
                ["database"] = _dbContext.Database.GetConnectionString() ?? "Unknown",
                ["duration"] = stopwatch.ElapsedMilliseconds,
                ["provider"] = _dbContext.Database.ProviderName ?? "Unknown"
            };

            return HealthCheckResult.Healthy(
                "Database is responsive",
                data: data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            
            return HealthCheckResult.Unhealthy(
                "Database health check failed",
                ex,
                data: new Dictionary<string, object>
                {
                    ["database"] = _dbContext.Database.GetConnectionString() ?? "Unknown",
                    ["error"] = ex.Message
                });
        }
    }
}