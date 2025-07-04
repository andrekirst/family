using Family.Infrastructure.Resilience.HealthChecks;
using System.Net;

namespace Family.Infrastructure.Resilience.Tests.HealthChecks;

public class ExternalServiceHealthCheckTests
{
    private readonly IFixture _fixture;
    private readonly FakeLogger<ExternalServiceHealthCheck> _logger;
    private readonly HttpClient _httpClient;
    private readonly FakeHttpMessageHandler _messageHandler;

    public ExternalServiceHealthCheckTests()
    {
        _fixture = new Fixture();
        _logger = new FakeLogger<ExternalServiceHealthCheck>();
        _messageHandler = new FakeHttpMessageHandler();
        _httpClient = new HttpClient(_messageHandler);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenServiceReturnsOk_ShouldReturnHealthy()
    {
        // Arrange
        var serviceName = "test-service";
        var endpoint = "https://api.test.com/health";
        var timeout = TimeSpan.FromSeconds(5);

        _messageHandler.SetResponse(new HttpResponseMessage(HttpStatusCode.OK));

        var healthCheck = new ExternalServiceHealthCheck(_httpClient, _logger, serviceName, endpoint, timeout);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("test", healthCheck, HealthStatus.Unhealthy, new[] { "external" })
        };

        // Act
        var result = await healthCheck.CheckHealthAsync(context, CancellationToken.None);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be($"External service '{serviceName}' is responsive");
        result.Data.Should().ContainKey("service");
        result.Data.Should().ContainKey("endpoint");
        result.Data.Should().ContainKey("duration");
        result.Data.Should().ContainKey("status_code");
        
        result.Data["service"].Should().Be(serviceName);
        result.Data["endpoint"].Should().Be(endpoint);
        result.Data["status_code"].Should().Be(200);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenServiceReturnsError_ShouldReturnUnhealthy()
    {
        // Arrange
        var serviceName = "test-service";
        var endpoint = "https://api.test.com/health";
        var timeout = TimeSpan.FromSeconds(5);

        _messageHandler.SetResponse(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("Internal server error")
        });

        var healthCheck = new ExternalServiceHealthCheck(_httpClient, _logger, serviceName, endpoint, timeout);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("test", healthCheck, HealthStatus.Unhealthy, new[] { "external" })
        };

        // Act
        var result = await healthCheck.CheckHealthAsync(context, CancellationToken.None);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be($"External service '{serviceName}' returned status code InternalServerError");
        result.Data.Should().ContainKey("status_code");
        result.Data.Should().ContainKey("response_body");
        
        result.Data["status_code"].Should().Be(500);
        result.Data["response_body"].Should().Be("Internal server error");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenTimeout_ShouldReturnUnhealthy()
    {
        // Arrange
        var serviceName = "test-service";
        var endpoint = "https://api.test.com/health";
        var timeout = TimeSpan.FromMilliseconds(10);

        _messageHandler.SetDelay(TimeSpan.FromSeconds(1)); // Longer than timeout

        var healthCheck = new ExternalServiceHealthCheck(_httpClient, _logger, serviceName, endpoint, timeout);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("test", healthCheck, HealthStatus.Unhealthy, new[] { "external" })
        };

        // Act
        var result = await healthCheck.CheckHealthAsync(context, CancellationToken.None);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be($"External service '{serviceName}' health check timed out");
        result.Data.Should().ContainKey("timeout_ms");
        result.Data.Should().ContainKey("error");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenCancellationRequested_ShouldReturnUnhealthy()
    {
        // Arrange
        var serviceName = "test-service";
        var endpoint = "https://api.test.com/health";
        var timeout = TimeSpan.FromSeconds(10);

        var healthCheck = new ExternalServiceHealthCheck(_httpClient, _logger, serviceName, endpoint, timeout);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("test", healthCheck, HealthStatus.Unhealthy, new[] { "external" })
        };

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await healthCheck.CheckHealthAsync(context, cts.Token);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be($"External service '{serviceName}' health check was cancelled");
        result.Data.Should().ContainKey("error");
        result.Data["error"].Should().Be("Operation was cancelled");
    }

    [Fact]
    public async Task CheckHealthAsync_WhenHttpException_ShouldReturnUnhealthy()
    {
        // Arrange
        var serviceName = "test-service";
        var endpoint = "https://api.test.com/health";
        var timeout = TimeSpan.FromSeconds(5);

        _messageHandler.SetException(new HttpRequestException("Connection refused"));

        var healthCheck = new ExternalServiceHealthCheck(_httpClient, _logger, serviceName, endpoint, timeout);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("test", healthCheck, HealthStatus.Unhealthy, new[] { "external" })
        };

        // Act
        var result = await healthCheck.CheckHealthAsync(context, CancellationToken.None);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be($"External service '{serviceName}' health check failed");
        result.Exception.Should().BeOfType<HttpRequestException>();
        result.Data.Should().ContainKey("error");
    }
}

/// <summary>
/// Fake HTTP message handler for testing
/// </summary>
public class FakeHttpMessageHandler : HttpMessageHandler
{
    private HttpResponseMessage? _response;
    private Exception? _exception;
    private TimeSpan _delay = TimeSpan.Zero;

    public void SetResponse(HttpResponseMessage response)
    {
        _response = response;
        _exception = null;
    }

    public void SetException(Exception exception)
    {
        _exception = exception;
        _response = null;
    }

    public void SetDelay(TimeSpan delay)
    {
        _delay = delay;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_delay > TimeSpan.Zero)
        {
            await Task.Delay(_delay, cancellationToken);
        }

        if (_exception != null)
        {
            throw _exception;
        }

        return _response ?? new HttpResponseMessage(HttpStatusCode.OK);
    }
}