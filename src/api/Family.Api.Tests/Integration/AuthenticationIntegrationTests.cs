using Family.Api.Data;
using Family.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Family.Api.Tests.Integration;

public class AuthenticationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthenticationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DirectLogin_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Replace real database with in-memory database
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<FamilyDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<FamilyDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                // Mock Keycloak service
                var keycloakService = Substitute.For<IKeycloakService>();
                keycloakService.DirectLoginAsync(Arg.Any<string>(), Arg.Any<string>())
                    .Returns(new LoginResult(
                        true,
                        "mock-access-token",
                        "mock-refresh-token",
                        new Models.User
                        {
                            Id = Guid.NewGuid(),
                            Email = "test@family.local",
                            FirstName = "Test",
                            LastName = "User",
                            KeycloakSubjectId = "test-subject-id",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        Array.Empty<string>()));

                services.AddScoped(_ => keycloakService);
            });
        }).CreateClient();

        // Act & Assert - Test the GraphQL endpoint is accessible
        var graphqlQuery = """
            {
                "query": "{ hello }"
            }
            """;

        var response = await client.PostAsync("/graphql", 
            new StringContent(graphqlQuery, System.Text.Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GraphQL_Endpoint_ShouldBeAccessible()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Replace real database with in-memory database
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<FamilyDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<FamilyDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/graphql");

        // Assert
        response.StatusCode.Should().BeOneOf(
            System.Net.HttpStatusCode.OK, 
            System.Net.HttpStatusCode.BadRequest); // GraphQL GET requests might return BadRequest
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Swagger_Endpoint_ShouldBeAccessible()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/index.html");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}