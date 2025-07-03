namespace Family.Infrastructure.Caching.Configuration;

/// <summary>
/// Configuration settings for HybridCache implementation
/// </summary>
public class CacheConfiguration
{
    public const string SectionName = "Cache";

    /// <summary>
    /// Redis connection string for L2 cache
    /// </summary>
    public string? RedisConnectionString { get; set; }

    /// <summary>
    /// Default expiration time for cache entries
    /// </summary>
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Default local cache expiration time (L1)
    /// </summary>
    public TimeSpan DefaultLocalExpiration { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Maximum size of local cache in bytes
    /// </summary>
    public long MaxLocalCacheSize { get; set; } = 100 * 1024 * 1024; // 100MB

    /// <summary>
    /// Cache key prefix for the Family application
    /// </summary>
    public string KeyPrefix { get; set; } = "family";

    /// <summary>
    /// Enable cache compression for large objects
    /// </summary>
    public bool EnableCompression { get; set; } = true;

    /// <summary>
    /// Cache policies for different data types
    /// </summary>
    public Dictionary<string, CachePolicy> Policies { get; set; } = new()
    {
        ["user"] = new() { Expiration = TimeSpan.FromMinutes(10), LocalExpiration = TimeSpan.FromMinutes(2) },
        ["family"] = new() { Expiration = TimeSpan.FromMinutes(15), LocalExpiration = TimeSpan.FromMinutes(3) },
        ["config"] = new() { Expiration = TimeSpan.FromHours(1), LocalExpiration = TimeSpan.FromMinutes(10) },
        ["session"] = new() { Expiration = TimeSpan.FromMinutes(30), LocalExpiration = TimeSpan.FromMinutes(5) }
    };
}

/// <summary>
/// Cache policy for specific data types
/// </summary>
public class CachePolicy
{
    /// <summary>
    /// Total cache expiration time (L1 + L2)
    /// </summary>
    public TimeSpan Expiration { get; set; }

    /// <summary>
    /// Local cache expiration time (L1 only)
    /// </summary>
    public TimeSpan LocalExpiration { get; set; }

    /// <summary>
    /// Cache tags for invalidation
    /// </summary>
    public string[] Tags { get; set; } = [];
}