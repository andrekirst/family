using AutoFixture;
using Family.Infrastructure.Caching.Abstractions;
using Family.Infrastructure.Caching.Configuration;
using Family.Infrastructure.Caching.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.Redis;

namespace Family.Api.Tests.Integration;

public class CacheIntegrationTests : IAsyncLifetime
{
    private readonly IFixture _fixture = new Fixture();
    private RedisContainer? _redisContainer;
    private IServiceProvider? _serviceProvider;

    public async Task InitializeAsync()
    {
        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithPortBinding(6379, true)
            .Build();

        await _redisContainer.StartAsync();

        var services = new ServiceCollection();
        
        var configData = new Dictionary<string, string?>
        {
            ["Cache:RedisConnectionString"] = _redisContainer.GetConnectionString(),
            ["Cache:DefaultExpiration"] = "00:05:00",
            ["Cache:KeyPrefix"] = "integration-test",
            ["Cache:Policies:user:Expiration"] = "00:10:00",
            ["Cache:Policies:user:Tags:0"] = "users"
        };
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddFamilyCaching(configuration);
        services.AddLogging(builder => builder.AddConsole());

        _serviceProvider = services.BuildServiceProvider();
    }

    public async Task DisposeAsync()
    {
        _serviceProvider?.Dispose();
        if (_redisContainer != null)
        {
            await _redisContainer.DisposeAsync();
        }
    }

    [Fact]
    public async Task CacheService_GetOrCreateAsync_ShouldCacheAndRetrieveData()
    {
        // Arrange
        var cacheService = _serviceProvider!.GetRequiredService<ICacheService>();
        var testData = _fixture.Create<TestCacheItem>();
        var factoryCallCount = 0;

        Func<CancellationToken, Task<TestCacheItem>> factory = _ =>
        {
            factoryCallCount++;
            return Task.FromResult(testData);
        };

        // Act - First call should invoke factory
        var result1 = await cacheService.GetOrCreateAsync("user", "test-key", factory);
        
        // Act - Second call should use cache
        var result2 = await cacheService.GetOrCreateAsync("user", "test-key", factory);

        // Assert
        result1.Should().BeEquivalentTo(testData);
        result2.Should().BeEquivalentTo(testData);
        factoryCallCount.Should().Be(1, "Factory should only be called once due to caching");
    }

    [Fact]
    public async Task CacheService_SetAndRemove_ShouldWorkCorrectly()
    {
        // Arrange
        var cacheService = _serviceProvider!.GetRequiredService<ICacheService>();
        var testData = _fixture.Create<TestCacheItem>();
        var factoryCallCount = 0;

        Func<CancellationToken, Task<TestCacheItem?>> factory = _ =>
        {
            factoryCallCount++;
            return Task.FromResult<TestCacheItem?>(null);
        };

        // Act - Set data in cache
        await cacheService.SetAsync("user", "test-key", testData);

        // Verify data is cached
        var cachedResult = await cacheService.GetOrCreateAsync("user", "test-key", factory);
        
        // Remove from cache
        await cacheService.RemoveAsync("user", "test-key");
        
        // Try to get again - should call factory
        var afterRemovalResult = await cacheService.GetOrCreateAsync("user", "test-key", factory);

        // Assert
        cachedResult.Should().BeEquivalentTo(testData);
        afterRemovalResult.Should().BeNull();
        factoryCallCount.Should().Be(1, "Factory should be called once after removal");
    }

    [Fact]
    public async Task CacheService_RemoveByTag_ShouldRemoveTaggedEntries()
    {
        // Arrange
        var cacheService = _serviceProvider!.GetRequiredService<ICacheService>();
        var testData1 = _fixture.Create<TestCacheItem>();
        var testData2 = _fixture.Create<TestCacheItem>();
        var factoryCallCount = 0;

        Func<CancellationToken, Task<TestCacheItem?>> factory = _ =>
        {
            factoryCallCount++;
            return Task.FromResult<TestCacheItem?>(null);
        };

        // Act - Set data with user tags
        await cacheService.SetAsync("user", "key1", testData1, ["custom-tag"]);
        await cacheService.SetAsync("user", "key2", testData2, ["other-tag"]);

        // Verify both are cached
        var cached1 = await cacheService.GetOrCreateAsync("user", "key1", factory);
        var cached2 = await cacheService.GetOrCreateAsync("user", "key2", factory);

        // Remove by tag (users tag should remove all user entries)
        await cacheService.RemoveByTagAsync("users");

        // Try to get again - should call factory for both
        var afterRemoval1 = await cacheService.GetOrCreateAsync("user", "key1", factory);
        var afterRemoval2 = await cacheService.GetOrCreateAsync("user", "key2", factory);

        // Assert
        cached1.Should().BeEquivalentTo(testData1);
        cached2.Should().BeEquivalentTo(testData2);
        afterRemoval1.Should().BeNull();
        afterRemoval2.Should().BeNull();
        factoryCallCount.Should().Be(2, "Factory should be called twice after tag removal");
    }

    [Fact]
    public async Task CacheService_WithExpiration_ShouldExpireAfterTimeout()
    {
        // Arrange
        var cacheService = _serviceProvider!.GetRequiredService<ICacheService>();
        var testData = _fixture.Create<TestCacheItem>();
        var factoryCallCount = 0;

        Func<CancellationToken, Task<TestCacheItem?>> factory = _ =>
        {
            factoryCallCount++;
            return Task.FromResult<TestCacheItem?>(testData);
        };

        // Act - Cache with very short expiration
        await cacheService.SetAsync("short-lived", "test-key", testData, null, TimeSpan.FromMilliseconds(100));
        
        // Verify it's cached
        var result1 = await cacheService.GetOrCreateAsync("short-lived", "test-key", factory);
        
        // Wait for expiration
        await Task.Delay(150);
        
        // Try to get again - should call factory due to expiration
        var result2 = await cacheService.GetOrCreateAsync("short-lived", "test-key", factory);

        // Assert
        result1.Should().BeEquivalentTo(testData);
        result2.Should().BeEquivalentTo(testData);
        factoryCallCount.Should().Be(1, "Factory should be called once after expiration");
    }

    [Fact]
    public void CacheService_BuildKey_ShouldCreateCorrectFormat()
    {
        // Arrange
        var cacheService = _serviceProvider!.GetRequiredService<ICacheService>();

        // Act
        var key = cacheService.BuildKey("user", "123");

        // Assert
        key.Should().Be("integration-test:user:123");
    }

    private class TestCacheItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<string> Tags { get; set; } = [];
    }
}