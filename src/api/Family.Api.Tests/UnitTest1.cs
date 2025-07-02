using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace Family.Api.Tests;

public class FamilyApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public FamilyApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact] 
    public async Task Get_Root_ReturnsNotFound()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_Swagger_ReturnsSuccess_InDevelopment()
    {
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
        }).CreateClient();

        var response = await client.GetAsync("/swagger/index.html");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}