using AutoFixture;
using Family.Api.Data;
using Family.Api.Models;
using Family.Api.Services;
using Family.Infrastructure.Caching.Abstractions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace Family.Api.Tests.Services;

public class CachedKeycloakServiceTests : IDisposable
{
    private readonly IFixture _fixture;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly FamilyDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachedKeycloakService> _logger;
    private readonly CachedKeycloakService _sut;

    public CachedKeycloakServiceTests()
    {
        _fixture = new Fixture();
        _httpClient = Substitute.For<HttpClient>();
        _cacheService = Substitute.For<ICacheService>();
        _logger = Substitute.For<ILogger<CachedKeycloakService>>();

        // Setup in-memory database
        var options = new DbContextOptionsBuilder<FamilyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FamilyDbContext(options);

        // Setup configuration
        var configData = new Dictionary<string, string?>
        {
            ["Keycloak:Authority"] = "http://localhost:8080/realms/family",
            ["Keycloak:ClientId"] = "family-api",
            ["Keycloak:ClientSecret"] = "test-secret",
            ["Keycloak:TokenEndpoint"] = "http://localhost:8080/realms/family/protocol/openid-connect/token",
            ["Keycloak:RedirectUri"] = "http://localhost:8081/auth/callback"
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _sut = new CachedKeycloakService(_httpClient, _configuration, _context, _cacheService, _logger);
    }

    [Fact]
    public async Task GetUserFromCacheAsync_WhenUserExists_ShouldReturnCachedUser()
    {
        // Arrange
        var keycloakSubjectId = "test-subject-id";
        var expectedUser = _fixture.Build<User>()
            .With(u => u.KeycloakSubjectId, keycloakSubjectId)
            .Create();

        _cacheService.GetOrCreateAsync(
            "user",
            $"keycloak:{keycloakSubjectId}",
            Arg.Any<Func<CancellationToken, Task<User?>>>(),
            Arg.Any<string[]>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedUser);

        // Act
        var result = await _sut.GetUserFromCacheAsync(keycloakSubjectId);

        // Assert
        result.Should().NotBeNull();
        result!.KeycloakSubjectId.Should().Be(keycloakSubjectId);
        result.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task GetUserFromCacheAsync_WhenUserNotInCache_ShouldLoadFromDatabase()
    {
        // Arrange
        var keycloakSubjectId = "test-subject-id";
        var dbUser = _fixture.Build<User>()
            .With(u => u.KeycloakSubjectId, keycloakSubjectId)
            .Create();

        _context.Users.Add(dbUser);
        await _context.SaveChangesAsync();

        _cacheService.GetOrCreateAsync(
            "user",
            $"keycloak:{keycloakSubjectId}",
            Arg.Any<Func<CancellationToken, Task<User?>>>(),
            Arg.Any<string[]>(),
            Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var factory = callInfo.Arg<Func<CancellationToken, Task<User?>>>();
                return factory(CancellationToken.None);
            });

        // Act
        var result = await _sut.GetUserFromCacheAsync(keycloakSubjectId);

        // Assert
        result.Should().NotBeNull();
        result!.KeycloakSubjectId.Should().Be(keycloakSubjectId);
        result.Email.Should().Be(dbUser.Email);
    }

    [Fact]
    public async Task GetUserByIdFromCacheAsync_WhenUserExists_ShouldReturnCachedUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedUser = _fixture.Build<User>()
            .With(u => u.Id, userId)
            .Create();

        _cacheService.GetOrCreateAsync(
            "user",
            $"id:{userId}",
            Arg.Any<Func<CancellationToken, Task<User?>>>(),
            Arg.Any<string[]>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedUser);

        // Act
        var result = await _sut.GetUserByIdFromCacheAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task SyncUserFromKeycloakAsync_WithNewUser_ShouldCreateAndCacheUser()
    {
        // Arrange
        var accessToken = CreateValidJwtToken("new-subject-id", "new@example.com", "John", "Doe");

        _cacheService.GetOrCreateAsync(
            "user",
            "keycloak:new-subject-id",
            Arg.Any<Func<CancellationToken, Task<User?>>>(),
            Arg.Any<string[]>(),
            Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var factory = callInfo.Arg<Func<CancellationToken, Task<User?>>>();
                return factory(CancellationToken.None);
            });

        // Act
        var result = await _sut.SyncUserFromKeycloakAsync(accessToken);

        // Assert
        result.Should().NotBeNull();
        result!.KeycloakSubjectId.Should().Be("new-subject-id");
        result.Email.Should().Be("new@example.com");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");

        // Verify user was added to database
        var dbUser = await _context.Users
            .FirstOrDefaultAsync(u => u.KeycloakSubjectId == "new-subject-id");
        dbUser.Should().NotBeNull();

        // Verify cache operations
        await _cacheService.Received().GetOrCreateAsync(
            "user",
            "keycloak:new-subject-id",
            Arg.Any<Func<CancellationToken, Task<User?>>>(),
            Arg.Is<string[]>(tags => tags.Contains("users")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncUserFromKeycloakAsync_WithExistingUser_ShouldUpdateAndCacheUser()
    {
        // Arrange
        var existingUser = _fixture.Build<User>()
            .With(u => u.KeycloakSubjectId, "existing-subject-id")
            .With(u => u.Email, "old@example.com")
            .Create();

        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var accessToken = CreateValidJwtToken("existing-subject-id", "updated@example.com", "Jane", "Smith");

        _cacheService.GetOrCreateAsync(
            "user",
            "keycloak:existing-subject-id",
            Arg.Any<Func<CancellationToken, Task<User?>>>(),
            Arg.Any<string[]>(),
            Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var factory = callInfo.Arg<Func<CancellationToken, Task<User?>>>();
                return factory(CancellationToken.None);
            });

        // Act
        var result = await _sut.SyncUserFromKeycloakAsync(accessToken);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("updated@example.com");
        result.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Smith");

        // Verify database was updated
        var dbUser = await _context.Users
            .FirstOrDefaultAsync(u => u.KeycloakSubjectId == "existing-subject-id");
        dbUser!.Email.Should().Be("updated@example.com");
    }

    [Fact]
    public async Task SyncUserFromKeycloakAsync_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var result = await _sut.SyncUserFromKeycloakAsync(invalidToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void InitiateLoginAsync_ShouldReturnValidLoginUrl()
    {
        // Act
        var result = _sut.InitiateLoginAsync().Result;

        // Assert
        result.Should().NotBeNull();
        result.LoginUrl.Should().Contain("http://localhost:8080/realms/family/protocol/openid-connect/auth");
        result.LoginUrl.Should().Contain("client_id=family-api");
        result.LoginUrl.Should().Contain("response_type=code");
        result.State.Should().NotBeNullOrEmpty();
        result.State.Length.Should().Be(32);
    }

    private string CreateValidJwtToken(string subjectId, string email, string firstName, string lastName)
    {
        var claims = new[]
        {
            new Claim("sub", subjectId),
            new Claim("email", email),
            new Claim("given_name", firstName),
            new Claim("family_name", lastName),
            new Claim("locale", "de"),
            new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim("exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: "http://localhost:8080/realms/family",
            audience: "family-api",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: null // For testing, we don't need actual signing
        );

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(token);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}