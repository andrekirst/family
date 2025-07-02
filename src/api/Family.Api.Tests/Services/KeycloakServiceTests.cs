using Family.Api.Data;
using Family.Api.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using System.Text;

namespace Family.Api.Tests.Services;

public class KeycloakServiceTests : IDisposable
{
    private readonly FamilyDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KeycloakService> _logger;
    private readonly HttpClient _httpClient;

    public KeycloakServiceTests()
    {
        var options = new DbContextOptionsBuilder<FamilyDbContext>()
            .UseInMemory(Guid.NewGuid().ToString())
            .Options;
        
        _context = new FamilyDbContext(options);
        _logger = Substitute.For<ILogger<KeycloakService>>();
        
        var configData = new Dictionary<string, string?>
        {
            ["Keycloak:Authority"] = "http://localhost:8080/realms/family",
            ["Keycloak:ClientId"] = "family-api",
            ["Keycloak:ClientSecret"] = "test-secret",
            ["Keycloak:TokenEndpoint"] = "http://localhost:8080/realms/family/protocol/openid-connect/token"
        };
        
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
        
        _httpClient = new HttpClient();
    }

    [Fact]
    public async Task InitiateLoginAsync_ShouldReturnLoginUrl()
    {
        // Arrange
        var service = new KeycloakService(_httpClient, _configuration, _context, _logger);

        // Act
        var result = await service.InitiateLoginAsync();

        // Assert
        result.LoginUrl.Should().StartWith("http://localhost:8080/realms/family/protocol/openid-connect/auth");
        result.LoginUrl.Should().Contain("client_id=family-api");
        result.LoginUrl.Should().Contain("response_type=code");
        result.LoginUrl.Should().Contain("scope=openid+profile+email");
        result.State.Should().NotBeNullOrEmpty();
        result.State.Length.Should().Be(32);
    }

    [Fact]
    public async Task DirectLoginAsync_WithInvalidCredentials_ShouldReturnFailure()
    {
        // Arrange
        var handler = new MockHttpMessageHandler();
        handler.SetResponse(HttpStatusCode.Unauthorized, """{"error": "invalid_grant"}""");
        
        var httpClient = new HttpClient(handler);
        var service = new KeycloakService(httpClient, _configuration, _context, _logger);

        // Act
        var result = await service.DirectLoginAsync("invalid@test.com", "wrongpassword");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.AccessToken.Should().BeNull();
        result.RefreshToken.Should().BeNull();
        result.User.Should().BeNull();
        result.Errors.Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange
        var handler = new MockHttpMessageHandler();
        handler.SetResponse(HttpStatusCode.OK, """
            {
                "access_token": "new-access-token",
                "refresh_token": "new-refresh-token",
                "token_type": "Bearer",
                "expires_in": 1800
            }
            """);
        
        var httpClient = new HttpClient(handler);
        var service = new KeycloakService(httpClient, _configuration, _context, _logger);

        // Act
        var result = await service.RefreshTokenAsync("old-refresh-token");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-refresh-token");
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task LogoutAsync_ShouldReturnSuccess()
    {
        // Arrange
        var handler = new MockHttpMessageHandler();
        handler.SetResponse(HttpStatusCode.NoContent, "");
        
        var httpClient = new HttpClient(handler);
        var service = new KeycloakService(httpClient, _configuration, _context, _logger);

        // Act
        var result = await service.LogoutAsync("access-token");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    public void Dispose()
    {
        _context.Dispose();
        _httpClient.Dispose();
    }
}

// Helper class for mocking HTTP responses
public class MockHttpMessageHandler : HttpMessageHandler
{
    private HttpStatusCode _statusCode = HttpStatusCode.OK;
    private string _content = "";

    public void SetResponse(HttpStatusCode statusCode, string content)
    {
        _statusCode = statusCode;
        _content = content;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_content, Encoding.UTF8, "application/json")
        };

        return Task.FromResult(response);
    }
}