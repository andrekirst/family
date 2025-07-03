using Microsoft.Extensions.Caching.Hybrid;

namespace Family.Infrastructure.Caching.Abstractions;

/// <summary>
/// High-level cache service abstraction for Family application
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets or creates a cache entry with the specified key and factory function
    /// </summary>
    /// <typeparam name="T">The type of the cached value</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="factory">Factory function to create the value if not cached</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cached or newly created value</returns>
    Task<T?> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets or creates a cache entry with custom options
    /// </summary>
    /// <typeparam name="T">The type of the cached value</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="factory">Factory function to create the value if not cached</param>
    /// <param name="options">Cache entry options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cached or newly created value</returns>
    Task<T?> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, HybridCacheEntryOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets or creates a cache entry for a specific data type using predefined policies
    /// </summary>
    /// <typeparam name="T">The type of the cached value</typeparam>
    /// <param name="dataType">The data type (e.g., "user", "family", "config")</param>
    /// <param name="key">The cache key suffix</param>
    /// <param name="factory">Factory function to create the value if not cached</param>
    /// <param name="tags">Additional cache tags for invalidation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cached or newly created value</returns>
    Task<T?> GetOrCreateAsync<T>(string dataType, string key, Func<CancellationToken, Task<T>> factory, string[]? tags = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a specific cache entry
    /// </summary>
    /// <param name="key">The cache key to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes cache entries by tag
    /// </summary>
    /// <param name="tag">The tag to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveByTagAsync(string tag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes cache entries by multiple tags
    /// </summary>
    /// <param name="tags">The tags to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveByTagsAsync(string[] tags, CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds a cache key with the configured prefix
    /// </summary>
    /// <param name="key">The base key</param>
    /// <returns>The prefixed cache key</returns>
    string BuildKey(string key);

    /// <summary>
    /// Builds a cache key for a specific data type
    /// </summary>
    /// <param name="dataType">The data type</param>
    /// <param name="key">The key suffix</param>
    /// <returns>The complete cache key</returns>
    string BuildKey(string dataType, string key);
}