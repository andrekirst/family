using Family.Infrastructure.EventSourcing.Data;
using Family.Infrastructure.EventSourcing.Services;
using Family.Infrastructure.EventSourcing.Tests.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Family.Infrastructure.EventSourcing.Tests.Services;

public class EventStoreTests : IDisposable
{
    private readonly EventSourcingDbContext _context;
    private readonly ILogger<EventStore> _logger;
    private readonly EventStore _eventStore;

    public EventStoreTests()
    {
        var options = new DbContextOptionsBuilder<EventSourcingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new EventSourcingDbContext(options);
        _logger = Substitute.For<ILogger<EventStore>>();
        _eventStore = new EventStore(_context, _logger);
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

    [Fact]
    public async Task GetEventsAsync_ShouldReturnEventsInOrder()
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
        var events = await _eventStore.GetEventsAsync(aggregateId);

        // Assert
        events.Should().HaveCount(2);
        events.First().Version.Should().Be(1);
        events.Last().Version.Should().Be(2);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}