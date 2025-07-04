using Family.Infrastructure.Resilience.Services;
using Family.Infrastructure.Resilience.Configuration;
using Microsoft.Extensions.Options;
using Polly.Registry;
using Polly;
using Polly.Timeout;

namespace Family.Infrastructure.Resilience.Tests.Services;

public class ResilienceServiceTests
{
    private readonly IFixture _fixture;
    private readonly FakeLogger<ResilienceService> _logger;
    private readonly ResiliencePipelineProvider<string> _pipelineProvider;
    private readonly ResilienceService _resilienceService;

    public ResilienceServiceTests()
    {
        _fixture = new Fixture();
        _logger = new FakeLogger<ResilienceService>();
        
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddResiliencePipeline("test", builder =>
        {
            builder.AddTimeout(TimeSpan.FromSeconds(1))
                   .AddRetry(new Polly.Retry.RetryStrategyOptions
                   {
                       MaxRetryAttempts = 2,
                       Delay = TimeSpan.FromMilliseconds(100)
                   });
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        _pipelineProvider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();
        _resilienceService = new ResilienceService(_pipelineProvider, _logger);
    }

    [Fact]
    public async Task ExecuteAsync_WithSuccessfulOperation_ShouldReturnResult()
    {
        // Arrange
        var expectedResult = _fixture.Create<string>();
        
        Func<CancellationToken, Task<string>> operation = _ => Task.FromResult(expectedResult);

        // Act
        var result = await _resilienceService.ExecuteAsync(operation, "test");

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task ExecuteAsync_WithFailingOperation_ShouldRetry()
    {
        // Arrange
        var callCount = 0;
        var expectedResult = _fixture.Create<string>();

        Func<CancellationToken, Task<string>> operation = _ =>
        {
            callCount++;
            if (callCount < 3) // Fail twice, succeed on third attempt
            {
                throw new InvalidOperationException("Test failure");
            }
            return Task.FromResult(expectedResult);
        };

        // Act
        var result = await _resilienceService.ExecuteAsync(operation, "test");

        // Assert
        result.Should().Be(expectedResult);
        callCount.Should().Be(3); // Original call + 2 retries
    }

    [Fact]
    public async Task ExecuteAsync_WithContinuousFailure_ShouldThrowAfterMaxRetries()
    {
        // Arrange
        var callCount = 0;
        var expectedException = new InvalidOperationException("Persistent failure");

        Func<CancellationToken, Task<string>> operation = _ =>
        {
            callCount++;
            throw expectedException;
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _resilienceService.ExecuteAsync(operation, "test"));

        exception.Should().Be(expectedException);
        callCount.Should().Be(3); // Original call + 2 retries
    }

    [Fact]
    public async Task ExecuteAsync_WithVoidOperation_ShouldExecuteSuccessfully()
    {
        // Arrange
        var executed = false;
        
        Func<CancellationToken, Task> operation = _ =>
        {
            executed = true;
            return Task.CompletedTask;
        };

        // Act
        await _resilienceService.ExecuteAsync(operation, "test");

        // Assert
        executed.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithTimeout_ShouldThrowTimeoutException()
    {
        // Arrange
        Func<CancellationToken, Task<string>> operation = async ct =>
        {
            await Task.Delay(TimeSpan.FromSeconds(2), ct); // Longer than 1 second timeout
            return "result";
        };

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutRejectedException>(
            () => _resilienceService.ExecuteAsync(operation, "test"));
    }

    [Fact]
    public void GetPipeline_ShouldReturnPipelineForKey()
    {
        // Act
        var pipeline = _resilienceService.GetPipeline("test");

        // Assert
        pipeline.Should().NotBeNull();
    }

    [Fact]
    public void GetPipeline_Generic_ShouldReturnTypedPipelineForKey()
    {
        // Act
        var pipeline = _resilienceService.GetPipeline<string>("test");

        // Assert
        pipeline.Should().NotBeNull();
    }

    [Fact]
    public void GetPipeline_WithNonExistentKey_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _resilienceService.GetPipeline("nonexistent"));
    }
}