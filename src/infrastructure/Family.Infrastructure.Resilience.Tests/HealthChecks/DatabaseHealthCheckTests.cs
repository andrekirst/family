using Family.Infrastructure.Resilience.HealthChecks;
using Microsoft.EntityFrameworkCore;

namespace Family.Infrastructure.Resilience.Tests.HealthChecks;

public class DatabaseHealthCheckTests
{
    private readonly IFixture _fixture;
    private readonly FakeDbContext _dbContext;
    private readonly FakeLogger<DatabaseHealthCheck<FakeDbContext>> _logger;

    public DatabaseHealthCheckTests()
    {
        _fixture = new Fixture();
        _logger = new FakeLogger<DatabaseHealthCheck<FakeDbContext>>();
        
        var options = new DbContextOptionsBuilder<FakeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _dbContext = new FakeDbContext(options);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenDatabaseIsConnected_ShouldReturnHealthy()
    {
        // Arrange
        var healthCheck = new DatabaseHealthCheck<FakeDbContext>(_dbContext, _logger);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("test", healthCheck, HealthStatus.Unhealthy, new[] { "database" })
        };

        // Act
        var result = await healthCheck.CheckHealthAsync(context, CancellationToken.None);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("Database is responsive");
        result.Data.Should().ContainKey("database");
        result.Data.Should().ContainKey("duration");
        result.Data.Should().ContainKey("provider");
        
        var durationMs = (long)result.Data["duration"];
        durationMs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenCancellationRequested_ShouldRespectCancellation()
    {
        // Arrange
        var healthCheck = new DatabaseHealthCheck<FakeDbContext>(_dbContext, _logger);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("test", healthCheck, HealthStatus.Unhealthy, new[] { "database" })
        };

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => healthCheck.CheckHealthAsync(context, cts.Token));
    }

    [Fact] 
    public async Task CheckHealthAsync_WhenDatabaseConnectionThrows_ShouldReturnUnhealthy()
    {
        // Arrange
        var mockContext = Substitute.For<FakeDbContext>();
        mockContext.Database.CanConnectAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(new InvalidOperationException("Connection failed")));

        var healthCheck = new DatabaseHealthCheck<FakeDbContext>(mockContext, _logger);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("test", healthCheck, HealthStatus.Unhealthy, new[] { "database" })
        };

        // Act
        var result = await healthCheck.CheckHealthAsync(context, CancellationToken.None);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("Database health check failed");
        result.Exception.Should().BeOfType<InvalidOperationException>();
        result.Data.Should().ContainKey("error");
    }
}

/// <summary>
/// Fake DbContext for testing purposes
/// </summary>
public class FakeDbContext : DbContext
{
    public FakeDbContext() { }
    public FakeDbContext(DbContextOptions<FakeDbContext> options) : base(options) { }
    
    public DbSet<FakeEntity> FakeEntities { get; set; } = null!;
}

/// <summary>
/// Fake entity for testing purposes
/// </summary>
public class FakeEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}