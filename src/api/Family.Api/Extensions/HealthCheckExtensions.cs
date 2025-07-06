using Family.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Family.Api.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddApiHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var kafkaBootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        
        services.AddHealthChecks()
            .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
            .AddKafka(options =>
            {
                options.BootstrapServers = kafkaBootstrapServers;
            }, name: "kafka", tags: new[] { "ready", "kafka" });

        // Add health checks UI for development
        services.AddHealthChecksUI()
            .AddInMemoryStorage();

        return services;
    }
}