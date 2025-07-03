using Family.Infrastructure.Caching.Abstractions;
using Family.Infrastructure.Caching.Configuration;
using Family.Infrastructure.Caching.Services;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Family.Infrastructure.Caching.Extensions;

/// <summary>
/// Extension methods for configuring caching services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds HybridCache services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddFamilyCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure cache settings
        services.Configure<CacheConfiguration>(
            configuration.GetSection(CacheConfiguration.SectionName));

        var cacheConfig = configuration
            .GetSection(CacheConfiguration.SectionName)
            .Get<CacheConfiguration>() ?? new CacheConfiguration();

        // Add HybridCache with configuration
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = cacheConfig.DefaultExpiration,
                LocalCacheExpiration = cacheConfig.DefaultLocalExpiration
            };

            // Configure L1 cache size limit
            options.MaximumPayloadBytes = cacheConfig.MaxLocalCacheSize;
        });

        // Add Redis as L2 cache if connection string is provided
        if (!string.IsNullOrEmpty(cacheConfig.RedisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = cacheConfig.RedisConnectionString;
                options.InstanceName = cacheConfig.KeyPrefix;
            });
        }

        // Register cache service
        services.AddScoped<ICacheService, CacheService>();

        return services;
    }

    /// <summary>
    /// Adds HybridCache services with custom configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Action to configure cache options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddFamilyCaching(
        this IServiceCollection services,
        Action<CacheConfiguration> configureOptions)
    {
        var cacheConfig = new CacheConfiguration();
        configureOptions(cacheConfig);

        services.Configure<CacheConfiguration>(options => configureOptions(options));

        // Add HybridCache with configuration
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = cacheConfig.DefaultExpiration,
                LocalCacheExpiration = cacheConfig.DefaultLocalExpiration
            };

            options.MaximumPayloadBytes = cacheConfig.MaxLocalCacheSize;
        });

        // Add Redis as L2 cache if connection string is provided
        if (!string.IsNullOrEmpty(cacheConfig.RedisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = cacheConfig.RedisConnectionString;
                options.InstanceName = cacheConfig.KeyPrefix;
            });
        }

        // Register cache service
        services.AddScoped<ICacheService, CacheService>();

        return services;
    }
}