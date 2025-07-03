using AutoFixture;
using Family.Infrastructure.Caching.Abstractions;
using Family.Infrastructure.Caching.Configuration;
using Family.Infrastructure.Caching.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Family.Api.Tests.Services;

public class CacheServiceTests
{
    private readonly IFixture _fixture;
    private readonly IHybridCache _hybridCache;
    private readonly IOptions<CacheConfiguration> _cacheOptions;
    private readonly ILogger<CacheService> _logger;
    private readonly CacheService _sut;

    public CacheServiceTests()
    {
        _fixture = new Fixture();
        _hybridCache = Substitute.For<IHybridCache>();
        _logger = Substitute.For<ILogger<CacheService>>();
        
        var cacheConfig = new CacheConfiguration
        {
            DefaultExpiration = TimeSpan.FromMinutes(5),
            KeyPrefix = "family-test",
            Policies = new Dictionary<string, CachePolicy>
            {
                ["user"] = new() { Expiration = TimeSpan.FromMinutes(10), Tags = ["users"] },
                ["session"] = new() { Expiration = TimeSpan.FromMinutes(30), Tags = ["sessions"] }
            }
        };
        
        _cacheOptions = Options.Create(cacheConfig);
        _sut = new CacheService(_hybridCache, _cacheOptions, _logger);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithValidDataType_ShouldUseCorrectExpiration()
    {
        // Arrange
        var dataType = "user";
        var key = "test-key";
        var expectedValue = _fixture.Create<TestCacheItem>();
        var factory = Substitute.For<Func<CancellationToken, Task<TestCacheItem>>>();
        factory.Invoke(Arg.Any<CancellationToken>()).Returns(expectedValue);

        _hybridCache.GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask<TestCacheItem>>>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedValue);

        // Act
        var result = await _sut.GetOrCreateAsync(dataType, key, factory);

        // Assert
        result.Should().Be(expectedValue);
        await _hybridCache.Received(1).GetOrCreateAsync(
            "family-test:user:test-key",
            Arg.Any<Func<CancellationToken, ValueTask<TestCacheItem>>>(),
            Arg.Is<HybridCacheEntryOptions>(opts => opts.Expiration == TimeSpan.FromMinutes(10)),
            Arg.Is<IEnumerable<string>>(tags => tags.Contains("users")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOrCreateAsync_WithUnknownDataType_ShouldUseDefaultExpiration()
    {
        // Arrange
        var dataType = "unknown";
        var key = "test-key";
        var expectedValue = _fixture.Create<TestCacheItem>();
        var factory = Substitute.For<Func<CancellationToken, Task<TestCacheItem>>>();
        factory.Invoke(Arg.Any<CancellationToken>()).Returns(expectedValue);

        _hybridCache.GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask<TestCacheItem>>>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedValue);

        // Act
        var result = await _sut.GetOrCreateAsync(dataType, key, factory);

        // Assert
        result.Should().Be(expectedValue);
        await _hybridCache.Received(1).GetOrCreateAsync(
            "family-test:unknown:test-key",
            Arg.Any<Func<CancellationToken, ValueTask<TestCacheItem>>>(),
            Arg.Is<HybridCacheEntryOptions>(opts => opts.Expiration == TimeSpan.FromMinutes(5)),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOrCreateAsync_WithCustomTags_ShouldCombineWithPolicyTags()
    {
        // Arrange
        var dataType = "user";
        var key = "test-key";
        var customTags = new[] { "custom-tag-1", "custom-tag-2" };
        var expectedValue = _fixture.Create<TestCacheItem>();
        var factory = Substitute.For<Func<CancellationToken, Task<TestCacheItem>>>();
        factory.Invoke(Arg.Any<CancellationToken>()).Returns(expectedValue);

        _hybridCache.GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask<TestCacheItem>>>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedValue);

        // Act
        var result = await _sut.GetOrCreateAsync(dataType, key, factory, customTags);

        // Assert
        result.Should().Be(expectedValue);
        await _hybridCache.Received(1).GetOrCreateAsync(
            "family-test:user:test-key",
            Arg.Any<Func<CancellationToken, ValueTask<TestCacheItem>>>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Is<IEnumerable<string>>(tags => 
                tags.Contains("users") && 
                tags.Contains("custom-tag-1") && 
                tags.Contains("custom-tag-2")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void BuildKey_ShouldCombinePrefixDataTypeAndKey()
    {
        // Arrange
        var dataType = "user";
        var key = "123";

        // Act
        var result = _sut.BuildKey(dataType, key);

        // Assert
        result.Should().Be("family-test:user:123");
    }

    [Fact]
    public async Task SetAsync_WithValidData_ShouldCallHybridCacheSet()
    {
        // Arrange
        var dataType = "user";
        var key = "test-key";
        var value = _fixture.Create<TestCacheItem>();
        var tags = new[] { "tag1", "tag2" };

        // Act
        await _sut.SetAsync(dataType, key, value, tags);

        // Assert
        await _hybridCache.Received(1).SetAsync(
            "family-test:user:test-key",
            value,
            Arg.Is<HybridCacheEntryOptions>(opts => opts.Expiration == TimeSpan.FromMinutes(10)),
            Arg.Is<IEnumerable<string>>(t => t.Contains("users") && t.Contains("tag1") && t.Contains("tag2")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveAsync_ShouldCallHybridCacheRemove()
    {
        // Arrange
        var dataType = "user";
        var key = "test-key";

        // Act
        await _sut.RemoveAsync(dataType, key);

        // Assert
        await _hybridCache.Received(1).RemoveAsync("family-test:user:test-key", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveByTagAsync_ShouldCallHybridCacheRemoveByTag()
    {
        // Arrange
        var tag = "users";

        // Act
        await _sut.RemoveByTagAsync(tag);

        // Assert
        await _hybridCache.Received(1).RemoveByTagAsync(tag, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenFactoryThrows_ShouldPropagateException()
    {
        // Arrange
        var dataType = "user";
        var key = "test-key";
        var expectedException = new InvalidOperationException("Test exception");
        var factory = Substitute.For<Func<CancellationToken, Task<TestCacheItem>>>();
        factory.Invoke(Arg.Any<CancellationToken>()).ThrowsAsync(expectedException);

        _hybridCache.GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, ValueTask<TestCacheItem>>>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(expectedException);

        // Act & Assert
        var act = async () => await _sut.GetOrCreateAsync(dataType, key, factory);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Test exception");
    }

    [Fact]
    public async Task GetOrCreateAsync_WithNullFactory_ShouldThrowArgumentNullException()
    {
        // Arrange
        var dataType = "user";
        var key = "test-key";
        Func<CancellationToken, Task<TestCacheItem>>? factory = null;

        // Act & Assert
        var act = async () => await _sut.GetOrCreateAsync(dataType, key, factory!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    private class TestCacheItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}