using Family.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Family.Api.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddApiHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

        // Add health checks UI for development
        services.AddHealthChecksUI()
            .AddInMemoryStorage();

        return services;
    }
}