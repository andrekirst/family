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

        return await GetOrCreateInternalAsync(fullKey, factory, options, cancellationToken);
    }

    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        HybridCacheEntryOptions options,
        CancellationToken cancellationToken = default)
    {
        var fullKey = BuildKey(key);
        return await GetOrCreateInternalAsync(fullKey, factory, options, cancellationToken);
    }

    public async Task<T?> GetOrCreateAsync<T>(
        string dataType,
        string key,
        Func<CancellationToken, Task<T>> factory,
        string[]? tags = null,
        CancellationToken cancellationToken = default)
    {
        var fullKey = BuildKey(dataType, key);
        var options = CreateOptionsForDataType(dataType, tags);

        return await GetOrCreateInternalAsync(fullKey, factory, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = BuildKey(key);
        
        try
        {
            await _hybridCache.RemoveAsync(fullKey, cancellationToken);
            _logger.LogDebug("Cache entry removed: {Key}", fullKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove cache entry: {Key}", fullKey);
        }
    }

    public async Task RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hybridCache.RemoveByTagAsync(tag, cancellationToken);
            _logger.LogDebug("Cache entries removed by tag: {Tag}", tag);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove cache entries by tag: {Tag}", tag);
        }
    }

    public async Task RemoveByTagsAsync(string[] tags, CancellationToken cancellationToken = default)
    {
        var tasks = tags.Select(tag => RemoveByTagAsync(tag, cancellationToken));
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

    private async Task<T?> GetOrCreateInternalAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        HybridCacheEntryOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _hybridCache.GetOrCreateAsync(key, factory, options, cancellationToken);
            
            if (result != null)
            {
                _logger.LogDebug("Cache hit for key: {Key}", key);
            }
            else
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache operation failed for key: {Key}", key);
            
            // Fallback to direct factory call if cache fails
            return await factory(cancellationToken);
        }
    }

    private HybridCacheEntryOptions CreateDefaultOptions()
    {
        return new HybridCacheEntryOptions
        {
            Expiration = _configuration.DefaultExpiration,
            LocalCacheExpiration = _configuration.DefaultLocalExpiration
        };
    }

    private HybridCacheEntryOptions CreateOptionsForDataType(string dataType, string[]? additionalTags = null)
    {
        var policy = _configuration.Policies.GetValueOrDefault(dataType);
        var options = policy != null
            ? new HybridCacheEntryOptions
            {
                Expiration = policy.Expiration,
                LocalCacheExpiration = policy.LocalExpiration
            }
            : CreateDefaultOptions();

        // Build tags
        var tags = new List<string> { dataType };
        
        if (policy?.Tags?.Length > 0)
        {
            tags.AddRange(policy.Tags);
        }
        
        if (additionalTags?.Length > 0)
        {
            tags.AddRange(additionalTags);
        }

        options.Tags = tags.ToArray();
        return options;
    }
}