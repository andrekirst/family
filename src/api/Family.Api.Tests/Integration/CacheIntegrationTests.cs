using AutoFixture;
using Family.Infrastructure.Caching.Abstractions;
using Family.Infrastructure.Caching.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Family.Api.Tests.Integration;

public class CacheIntegrationTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void CacheService_Registration_ShouldWorkCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        
        var configData = new Dictionary<string, string?>
        {
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

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var cacheService = serviceProvider.GetService<ICacheService>();

        // Assert
        cacheService.Should().NotBeNull();
        cacheService.Should().BeOfType<Family.Infrastructure.Caching.Services.CacheService>();
    }

    [Fact]
    public void CacheService_BuildKey_ShouldCreateCorrectFormat()
    {
        // Arrange
        var services = new ServiceCollection();
        
        var configData = new Dictionary<string, string?>
        {
            ["Cache:KeyPrefix"] = "test-prefix"
        };
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddFamilyCaching(configuration);
        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();
        var cacheService = serviceProvider.GetRequiredService<ICacheService>();

        // Act
        var key = cacheService.BuildKey("user", "123");

        // Assert
        key.Should().Be("test-prefix:user:123");
    }

    private class TestCacheItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}