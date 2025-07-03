using Family.Infrastructure.Caching.Abstractions;
using Family.Infrastructure.Caching.Configuration;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Family.Infrastructure.Caching.Services;

/// <summary>
/// Implementation of ICacheService using HybridCache
/// </summary>
public class CacheService : ICacheService
{
    private readonly HybridCache _hybridCache;
    private readonly CacheConfiguration _configuration;
    private readonly ILogger<CacheService> _logger;

    public CacheService(
        HybridCache hybridCache,
        IOptions<CacheConfiguration> configuration,
        ILogger<CacheService> logger)
    {
        _hybridCache = hybridCache;
        _configuration = configuration.Value;
        _logger = logger;
    }

    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        CancellationToken cancellationToken = default)
    {
        var fullKey = BuildKey(key);
        var options = CreateDefaultOptions();

        return await _hybridCache.GetOrCreateAsync<object, T>(
            fullKey,
            state: null!,
            factory: async (_, ct) => await factory(ct),
            options: options,
            cancellationToken: cancellationToken);
    }

    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        HybridCacheEntryOptions options,
        CancellationToken cancellationToken = default)
    {
        var fullKey = BuildKey(key);
        
        return await _hybridCache.GetOrCreateAsync<object, T>(
            fullKey,
            state: null!,
            factory: async (_, ct) => await factory(ct),
            options: options,
            cancellationToken: cancellationToken);
    }

    public async Task<T?> GetOrCreateAsync<T>(
        string dataType,
        string key,
        Func<CancellationToken, Task<T>> factory,
        string[]? tags = null,
        CancellationToken cancellationToken = default)
    {
        var fullKey = BuildKey(dataType, key);
        var options = CreateOptionsForDataType(dataType);

        return await _hybridCache.GetOrCreateAsync<object, T>(
            fullKey,
            state: null!,
            factory: async (_, ct) => await factory(ct),
            options: options,
            tags: tags,
            cancellationToken: cancellationToken);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        CancellationToken cancellationToken = default)
    {
        var fullKey = BuildKey(key);
        var options = CreateDefaultOptions();
        
        await _hybridCache.SetAsync(fullKey, value, options, cancellationToken: cancellationToken);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        HybridCacheEntryOptions options,
        CancellationToken cancellationToken = default)
    {
        var fullKey = BuildKey(key);
        await _hybridCache.SetAsync(fullKey, value, options, cancellationToken: cancellationToken);
    }

    public async Task SetAsync<T>(
        string dataType,
        string key,
        T value,
        string[]? tags = null,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var fullKey = BuildKey(dataType, key);
        var options = CreateOptionsForDataType(dataType, expiration);
        
        await _hybridCache.SetAsync(fullKey, value, options, tags, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = BuildKey(key);
        await _hybridCache.RemoveAsync(fullKey, cancellationToken);
    }

    public async Task RemoveAsync(string dataType, string key, CancellationToken cancellationToken = default)
    {
        var fullKey = BuildKey(dataType, key);
        await _hybridCache.RemoveAsync(fullKey, cancellationToken);
    }

    public async Task RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        await _hybridCache.RemoveByTagAsync(tag, cancellationToken);
    }

    public async Task RemoveByTagsAsync(string[] tags, CancellationToken cancellationToken = default)
    {
        var tasks = tags.Select(tag => _hybridCache.RemoveByTagAsync(tag, cancellationToken).AsTask());
        await Task.WhenAll(tasks);
    }

    public string BuildKey(string key)
    {
        return $"{_configuration.KeyPrefix}:{key}";
    }

    public string BuildKey(string dataType, string key)
    {
        return $"{_configuration.KeyPrefix}:{dataType}:{key}";
    }

    private HybridCacheEntryOptions CreateDefaultOptions()
    {
        return new HybridCacheEntryOptions
        {
            Expiration = _configuration.DefaultExpiration,
            LocalCacheExpiration = _configuration.DefaultLocalExpiration
        };
    }

    private HybridCacheEntryOptions CreateOptionsForDataType(string dataType, TimeSpan? customExpiration = null)
    {
        if (_configuration.Policies.TryGetValue(dataType, out var policy))
        {
            return new HybridCacheEntryOptions
            {
                Expiration = customExpiration ?? policy.Expiration,
                LocalCacheExpiration = policy.LocalExpiration
            };
        }

        return new HybridCacheEntryOptions
        {
            Expiration = customExpiration ?? _configuration.DefaultExpiration,
            LocalCacheExpiration = _configuration.DefaultLocalExpiration
        };
    }
}