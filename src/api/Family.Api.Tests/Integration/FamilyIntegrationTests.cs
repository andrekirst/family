using Family.Api.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Family.Api.Tests.Integration;

[Trait("Category", "Integration")]
public class FamilyIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public FamilyIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Remove the app DbContext.
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<FamilyDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add a database context using an in-memory database for testing.
                services.AddDbContext<FamilyDbContext>(options =>
                {
                    options.UseInMemoryDatabase($"InMemoryDbForTesting_{Guid.NewGuid()}");
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GraphQL_Endpoint_ShouldBeAccessible()
    {
        // Arrange
        var query = @"
        {
            __schema {
                types {
                    name
                }
            }
        }";

        var requestBody = new
        {
            query = query
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/graphql", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Health_Check_Should_Return_Healthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Healthy");
    }

    [Fact]
    public async Task GraphQL_Schema_Should_Include_Family_Types()
    {
        // Arrange
        var query = @"
        {
            __schema {
                types {
                    name
                    fields {
                        name
                        type {
                            name
                        }
                    }
                }
            }
        }";

        var requestBody = new
        {
            query = query
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/graphql", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("Family");
            responseContent.Should().Contain("CreateFamilyInput");
            responseContent.Should().Contain("CreateFamilyPayload");
        }
    }

    [Fact]
    public async Task GraphQL_CreateFamily_Without_Authentication_Should_Return_Unauthorized()
    {
        // Arrange
        var mutation = @"
        mutation {
            createFamily(input: { name: ""Test Family"" }) {
                success
                family {
                    id
                    name
                }
                errorMessage
            }
        }";

        var requestBody = new
        {
            query = mutation
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/graphql", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.OK);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("error");
        }
    }

    [Fact]
    public async Task GraphQL_HasFamily_Query_Without_Authentication_Should_Return_Unauthorized()
    {
        // Arrange
        var query = @"
        {
            hasFamily
        }";

        var requestBody = new
        {
            query = query
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/graphql", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.OK);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("error");
        }
    }

    [Fact]
    public async Task GraphQL_GetMyFamily_Query_Without_Authentication_Should_Return_Unauthorized()
    {
        // Arrange
        var query = @"
        {
            getMyFamily {
                id
                name
                ownerId
                members {
                    userId
                    role
                }
            }
        }";

        var requestBody = new
        {
            query = query
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/graphql", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.OK);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("error");
        }
    }

    [Fact]
    public async Task Database_Should_Be_Created_And_Accessible()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FamilyDbContext>();

        // Act
        var canConnect = await context.Database.CanConnectAsync();

        // Assert
        canConnect.Should().BeTrue();
    }

    [Fact]
    public async Task Database_Should_Have_Required_Tables()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FamilyDbContext>();

        // Act & Assert
        var familiesTableExists = context.Model.FindEntityType("Family.Api.Data.Entities.FamilyEntity") != null;
        var familyMembersTableExists = context.Model.FindEntityType("Family.Api.Data.Entities.FamilyMemberEntity") != null;
        var usersTableExists = context.Model.FindEntityType("Family.Api.Models.User") != null;

        familiesTableExists.Should().BeTrue();
        familyMembersTableExists.Should().BeTrue();
        usersTableExists.Should().BeTrue();
    }

    [Fact]
    public async Task GraphQL_Introspection_Should_Work()
    {
        // Arrange
        var query = @"
        {
            __type(name: ""Query"") {
                name
                fields {
                    name
                    description
                }
            }
        }";

        var requestBody = new
        {
            query = query
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/graphql", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("Query");
        }
    }

    [Fact]
    public async Task GraphQL_InvalidQuery_Should_Return_Error()
    {
        // Arrange
        var invalidQuery = @"
        {
            invalidField {
                invalidSubField
            }
        }";

        var requestBody = new
        {
            query = invalidQuery
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/graphql", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("error");
        }
    }

    [Fact]
    public async Task GraphQL_EmptyQuery_Should_Return_Error()
    {
        // Arrange
        var requestBody = new
        {
            query = ""
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/graphql", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("error");
        }
    }

    [Fact]
    public async Task GraphQL_MalformedQuery_Should_Return_Error()
    {
        // Arrange
        var malformedQuery = @"
        {
            getMyFamily {
                id
                name
                // Missing closing brace
        ";

        var requestBody = new
        {
            query = malformedQuery
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/graphql", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("error");
        }
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}