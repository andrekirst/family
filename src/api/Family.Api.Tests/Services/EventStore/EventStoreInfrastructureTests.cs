using Family.Api.Data;
using Family.Api.Models.EventStore;
using Family.Api.Services.EventStore;
using Family.Api.Tests.Models.EventStore;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Family.Api.Tests.Services.EventStore;

public class EventStoreInfrastructureTests : IDisposable
{
    private readonly FamilyDbContext _context;
    private readonly ILogger<Family.Api.Services.EventStore.EventStore> _logger;
    private readonly Family.Api.Services.EventStore.EventStore _eventStore;

    public EventStoreInfrastructureTests()
    {
        var options = new DbContextOptionsBuilder<FamilyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new FamilyDbContext(options);
        _logger = Substitute.For<ILogger<Family.Api.Services.EventStore.EventStore>>();
        _eventStore = new Family.Api.Services.EventStore.EventStore(_context, _logger);
    }

    [Fact]
    public async Task SaveEventAsync_ShouldPersistEventToDatabase()
    {
        // Arrange
        var domainEvent = new TestDomainEvent
        {
            EventId = Guid.NewGuid().ToString(),
            AggregateId = "aggregate-1",
            AggregateType = "TestAggregate",
            EventType = nameof(TestDomainEvent),
            Version = 1,
            Timestamp = DateTime.UtcNow,
            UserId = "user-1",
            CorrelationId = "correlation-1",
            CausationId = Guid.NewGuid().ToString(),
            Metadata = new Dictionary<string, object>(),
            TestData = "Test Data"
        };

        // Act
        await _eventStore.SaveEventAsync(domainEvent);

        // Assert
        var savedEvent = await _context.Events.FirstOrDefaultAsync();
        savedEvent.Should().NotBeNull();
        savedEvent!.AggregateId.Should().Be("aggregate-1");
        savedEvent.EventType.Should().Be(nameof(TestDomainEvent));
        savedEvent.Version.Should().Be(1);
    }

    [Fact]
    public async Task GetLatestVersionAsync_ShouldReturnCorrectVersion()
    {
        // Arrange
        var aggregateId = "aggregate-1";
        var event1 = new TestDomainEvent
        {
            EventId = Guid.NewGuid().ToString(),
            AggregateId = aggregateId,
            AggregateType = "TestAggregate",
            EventType = nameof(TestDomainEvent),
            Version = 1,
            Timestamp = DateTime.UtcNow,
            UserId = "user-1",
            CorrelationId = "correlation-1",
            CausationId = Guid.NewGuid().ToString(),
            Metadata = new Dictionary<string, object>(),
            TestData = "Event 1"
        };

        var event2 = new TestDomainEvent
        {
            EventId = Guid.NewGuid().ToString(),
            AggregateId = aggregateId,
            AggregateType = "TestAggregate",
            EventType = nameof(TestDomainEvent),
            Version = 2,
            Timestamp = DateTime.UtcNow,
            UserId = "user-1",
            CorrelationId = "correlation-2",
            CausationId = Guid.NewGuid().ToString(),
            Metadata = new Dictionary<string, object>(),
            TestData = "Event 2"
        };

        await _eventStore.SaveEventAsync(event1);
        await _eventStore.SaveEventAsync(event2);

        // Act
        var latestVersion = await _eventStore.GetLatestVersionAsync(aggregateId);

        // Assert
        latestVersion.Should().Be(2);
    }

    [Fact]
    public async Task EventExistsAsync_WithExistingEvent_ShouldReturnTrue()
    {
        // Arrange
        var domainEvent = new TestDomainEvent
        {
            EventId = Guid.NewGuid().ToString(),
            AggregateId = "aggregate-1",
            AggregateType = "TestAggregate",
            EventType = nameof(TestDomainEvent),
            Version = 1,
            Timestamp = DateTime.UtcNow,
            UserId = "user-1",
            CorrelationId = "correlation-1",
            CausationId = Guid.NewGuid().ToString(),
            Metadata = new Dictionary<string, object>(),
            TestData = "Test Data"
        };
        await _eventStore.SaveEventAsync(domainEvent);

        // Act
        var exists = await _eventStore.EventExistsAsync(domainEvent.EventId);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task EventExistsAsync_WithNonExistentEvent_ShouldReturnFalse()
    {
        // Act
        var exists = await _eventStore.EventExistsAsync(Guid.NewGuid().ToString());

        // Assert
        exists.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}