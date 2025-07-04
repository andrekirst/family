using Family.Infrastructure.Resilience.Configuration;

namespace Family.Infrastructure.Resilience.Tests.Configuration;

public class ResilienceConfigurationTests
{
    private readonly IFixture _fixture;

    public ResilienceConfigurationTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void ResilienceConfiguration_ShouldHaveCorrectDefaults()
    {
        // Act
        var config = new ResilienceConfiguration();

        // Assert
        config.HealthChecks.Should().NotBeNull();
        config.CircuitBreaker.Should().NotBeNull();
        config.Retry.Should().NotBeNull();
        config.Timeout.Should().NotBeNull();
    }

    [Fact]
    public void HealthCheckConfiguration_ShouldHaveCorrectDefaults()
    {
        // Act
        var config = new HealthCheckConfiguration();

        // Assert
        config.HealthPath.Should().Be("/health");
        config.ReadyPath.Should().Be("/health/ready");
        config.LivePath.Should().Be("/health/live");
        config.EnableUI.Should().BeTrue();
        config.UIPath.Should().Be("/health-ui");
        config.EvaluationInterval.Should().Be(TimeSpan.FromSeconds(30));
        config.Timeout.Should().Be(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void CircuitBreakerConfiguration_ShouldHaveCorrectDefaults()
    {
        // Act
        var config = new CircuitBreakerConfiguration();

        // Assert
        config.HandledEventsAllowedBeforeBreaking.Should().Be(5);
        config.DurationOfBreak.Should().Be(TimeSpan.FromSeconds(30));
        config.MinimumThroughput.Should().Be(10);
        config.SamplingDuration.Should().Be(TimeSpan.FromSeconds(60));
    }

    [Fact]
    public void RetryConfiguration_ShouldHaveCorrectDefaults()
    {
        // Act
        var config = new RetryConfiguration();

        // Assert
        config.MaxRetryAttempts.Should().Be(3);
        config.BaseDelay.Should().Be(TimeSpan.FromSeconds(1));
        config.MaxDelay.Should().Be(TimeSpan.FromSeconds(30));
        config.UseJitter.Should().BeTrue();
    }

    [Fact]
    public void TimeoutConfiguration_ShouldHaveCorrectDefaults()
    {
        // Act
        var config = new TimeoutConfiguration();

        // Assert
        config.DefaultTimeout.Should().Be(TimeSpan.FromSeconds(30));
        config.DatabaseTimeout.Should().Be(TimeSpan.FromSeconds(10));
        config.ExternalApiTimeout.Should().Be(TimeSpan.FromSeconds(15));
        config.CacheTimeout.Should().Be(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ResilienceConfiguration_SectionName_ShouldBeCorrect()
    {
        // Act & Assert
        ResilienceConfiguration.SectionName.Should().Be("Resilience");
    }

    [Fact]
    public void HealthCheckConfiguration_CanBeConfigured()
    {
        // Arrange
        var config = new HealthCheckConfiguration();
        var customPath = _fixture.Create<string>();
        var customTimeout = TimeSpan.FromMinutes(1);

        // Act
        config.HealthPath = customPath;
        config.Timeout = customTimeout;
        config.EnableUI = false;

        // Assert
        config.HealthPath.Should().Be(customPath);
        config.Timeout.Should().Be(customTimeout);
        config.EnableUI.Should().BeFalse();
    }

    [Fact]
    public void CircuitBreakerConfiguration_CanBeConfigured()
    {
        // Arrange
        var config = new CircuitBreakerConfiguration();
        var customThreshold = _fixture.Create<int>();
        var customDuration = TimeSpan.FromMinutes(2);

        // Act
        config.HandledEventsAllowedBeforeBreaking = customThreshold;
        config.DurationOfBreak = customDuration;

        // Assert
        config.HandledEventsAllowedBeforeBreaking.Should().Be(customThreshold);
        config.DurationOfBreak.Should().Be(customDuration);
    }

    [Fact]
    public void RetryConfiguration_CanBeConfigured()
    {
        // Arrange
        var config = new RetryConfiguration();
        var customAttempts = _fixture.Create<int>();
        var customDelay = TimeSpan.FromMilliseconds(500);

        // Act
        config.MaxRetryAttempts = customAttempts;
        config.BaseDelay = customDelay;
        config.UseJitter = false;

        // Assert
        config.MaxRetryAttempts.Should().Be(customAttempts);
        config.BaseDelay.Should().Be(customDelay);
        config.UseJitter.Should().BeFalse();
    }

    [Fact]
    public void TimeoutConfiguration_CanBeConfigured()
    {
        // Arrange
        var config = new TimeoutConfiguration();
        var customTimeout = TimeSpan.FromMinutes(5);

        // Act
        config.DefaultTimeout = customTimeout;
        config.DatabaseTimeout = customTimeout;
        config.ExternalApiTimeout = customTimeout;
        config.CacheTimeout = customTimeout;

        // Assert
        config.DefaultTimeout.Should().Be(customTimeout);
        config.DatabaseTimeout.Should().Be(customTimeout);
        config.ExternalApiTimeout.Should().Be(customTimeout);
        config.CacheTimeout.Should().Be(customTimeout);
    }
}