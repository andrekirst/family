using Family.Infrastructure.Resilience.Abstractions;
using Family.Infrastructure.Resilience.Configuration;
using Family.Infrastructure.Resilience.HealthChecks;
using Family.Infrastructure.Resilience.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Polly.Registry;
using StackExchange.Redis;

namespace Family.Infrastructure.Resilience.Extensions;

/// <summary>
/// Extension methods for configuring resilience services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds resilience services to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration root</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddResilience(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure resilience settings
        services.Configure<ResilienceConfiguration>(
            configuration.GetSection(ResilienceConfiguration.SectionName));

        // Register resilience services
        services.AddSingleton<IResiliencePipelineFactory, ResiliencePipelineFactory>();
        services.AddScoped<IResilienceService, ResilienceService>();

        // Configure Polly resilience pipelines
        services.AddResiliencePipeline("default", builder =>
        {
            builder.AddTimeout(TimeSpan.FromSeconds(30))
                   .AddRetry(new Polly.Retry.RetryStrategyOptions
                   {
                       MaxRetryAttempts = 3,
                       Delay = TimeSpan.FromSeconds(1),
                       UseJitter = true
                   });
        });

        services.AddResiliencePipeline("database", builder =>
        {
            builder.AddTimeout(TimeSpan.FromSeconds(10))
                   .AddRetry(new Polly.Retry.RetryStrategyOptions
                   {
                       MaxRetryAttempts = 3,
                       Delay = TimeSpan.FromSeconds(1),
                       UseJitter = true
                   })
                   .AddCircuitBreaker(new Polly.CircuitBreaker.CircuitBreakerStrategyOptions
                   {
                       FailureRatio = 0.5,
                       MinimumThroughput = 10,
                       SamplingDuration = TimeSpan.FromSeconds(60),
                       BreakDuration = TimeSpan.FromSeconds(30)
                   });
        });

        services.AddResiliencePipeline("external-api", builder =>
        {
            builder.AddTimeout(TimeSpan.FromSeconds(15))
                   .AddRetry(new Polly.Retry.RetryStrategyOptions
                   {
                       MaxRetryAttempts = 3,
                       Delay = TimeSpan.FromSeconds(1),
                       UseJitter = true
                   })
                   .AddCircuitBreaker(new Polly.CircuitBreaker.CircuitBreakerStrategyOptions
                   {
                       FailureRatio = 0.6,
                       MinimumThroughput = 5,
                       SamplingDuration = TimeSpan.FromSeconds(60),
                       BreakDuration = TimeSpan.FromSeconds(30)
                   });
        });

        services.AddResiliencePipeline("cache", builder =>
        {
            builder.AddTimeout(TimeSpan.FromSeconds(5))
                   .AddRetry(new Polly.Retry.RetryStrategyOptions
                   {
                       MaxRetryAttempts = 2,
                       Delay = TimeSpan.FromMilliseconds(100),
                       UseJitter = true
                   });
        });

        return services;
    }

    /// <summary>
    /// Adds comprehensive health checks to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration root</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddFamilyHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var healthCheckConfig = configuration
            .GetSection($"{ResilienceConfiguration.SectionName}:HealthChecks")
            .Get<HealthCheckConfiguration>() ?? new HealthCheckConfiguration();

        var healthChecksBuilder = services.AddHealthChecks();

        // Add database health check
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            healthChecksBuilder.AddNpgSql(
                connectionString,
                name: "database",
                tags: ["ready", "database"],
                timeout: healthCheckConfig.Timeout);
        }

        // Add Redis health check
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            healthChecksBuilder.AddRedis(
                redisConnectionString,
                name: "redis",
                tags: ["ready", "cache"],
                timeout: healthCheckConfig.Timeout);
        }

        // Add Kafka health check
        var kafkaConnectionString = configuration.GetConnectionString("Kafka");
        if (!string.IsNullOrEmpty(kafkaConnectionString))
        {
            healthChecksBuilder.AddKafka(options =>
            {
                options.BootstrapServers = kafkaConnectionString;
            },
            name: "kafka",
            tags: ["ready", "messaging"],
            timeout: healthCheckConfig.Timeout);
        }

        // Add Keycloak health check
        var keycloakAuthority = configuration["Keycloak:Authority"];
        if (!string.IsNullOrEmpty(keycloakAuthority))
        {
            services.AddHttpClient<ExternalServiceHealthCheck>("keycloak", client =>
            {
                client.BaseAddress = new Uri(keycloakAuthority);
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            healthChecksBuilder.AddTypeActivatedCheck<ExternalServiceHealthCheck>(
                "keycloak",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "external"],
                timeout: healthCheckConfig.Timeout,
                args: [
                    "Keycloak",
                    $"{keycloakAuthority}",
                    TimeSpan.FromSeconds(10)
                ]);
        }

        // Configure Health Checks UI if enabled
        if (healthCheckConfig.EnableUI)
        {
            services.AddHealthChecksUI(options =>
            {
                options.SetEvaluationTimeInSeconds((int)healthCheckConfig.EvaluationInterval.TotalSeconds);
                options.AddHealthCheckEndpoint("Family API", "/health");
            })
            .AddInMemoryStorage();
        }

        return services;
    }

    /// <summary>
    /// Adds database-specific health checks
    /// </summary>
    /// <typeparam name="TDbContext">Database context type</typeparam>
    /// <param name="services">Service collection</param>
    /// <param name="name">Health check name</param>
    /// <param name="tags">Health check tags</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDatabaseHealthCheck<TDbContext>(
        this IServiceCollection services,
        string name = "database",
        string[]? tags = null)
        where TDbContext : DbContext
    {
        tags ??= ["ready", "database"];

        services.AddHealthChecks()
            .AddTypeActivatedCheck<DatabaseHealthCheck<TDbContext>>(
                name,
                failureStatus: HealthStatus.Unhealthy,
                tags: tags,
                timeout: TimeSpan.FromSeconds(10));

        return services;
    }

    /// <summary>
    /// Adds Redis-specific health checks
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="name">Health check name</param>
    /// <param name="tags">Health check tags</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddRedisHealthCheck(
        this IServiceCollection services,
        string name = "redis",
        string[]? tags = null)
    {
        tags ??= ["ready", "cache"];

        services.AddHealthChecks()
            .AddTypeActivatedCheck<RedisHealthCheck>(
                name,
                failureStatus: HealthStatus.Unhealthy,
                tags: tags,
                timeout: TimeSpan.FromSeconds(5));

        return services;
    }

    /// <summary>
    /// Adds external service health checks
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="serviceName">Service name</param>
    /// <param name="healthEndpoint">Health endpoint URL</param>
    /// <param name="timeout">Health check timeout</param>
    /// <param name="tags">Health check tags</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddExternalServiceHealthCheck(
        this IServiceCollection services,
        string serviceName,
        string healthEndpoint,
        TimeSpan? timeout = null,
        string[]? tags = null)
    {
        tags ??= ["ready", "external"];
        timeout ??= TimeSpan.FromSeconds(10);

        services.AddHttpClient<ExternalServiceHealthCheck>(serviceName.ToLowerInvariant(), client =>
        {
            client.Timeout = timeout.Value;
        });

        services.AddHealthChecks()
            .AddTypeActivatedCheck<ExternalServiceHealthCheck>(
                serviceName.ToLowerInvariant(),
                failureStatus: HealthStatus.Unhealthy,
                tags: tags,
                timeout: timeout.Value,
                args: [serviceName, healthEndpoint, timeout.Value]);

        return services;
    }
}