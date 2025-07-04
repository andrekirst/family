using Family.Api.Data;
using Family.Api.Models.EventStore;
using Family.Api.Models.EventStore.Events;
using Family.Api.Services.EventStore;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Family.Api.Tests.Services.EventStore;

public class EventStoreTests : IDisposable
{
    private readonly FamilyDbContext _context;
    private readonly ILogger<Family.Api.Services.EventStore.EventStore> _logger;
    private readonly Family.Api.Services.EventStore.EventStore _eventStore;

    public EventStoreTests()
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
        var domainEvent = DomainEvent.Create<FamilyCreatedEvent>(
            "family-1", "Family", 1, "user-1", "correlation-1") with
        {
            FamilyName = "Test Family",
            CreatedBy = "user-1"
        };

        // Act
        await _eventStore.SaveEventAsync(domainEvent);

        // Assert
        var savedEvent = await _context.Events.FirstOrDefaultAsync();
        savedEvent.Should().NotBeNull();
        savedEvent!.AggregateId.Should().Be("family-1");
        savedEvent.EventType.Should().Be(nameof(FamilyCreatedEvent));
        savedEvent.Version.Should().Be(1);
    }

    [Fact]
    public async Task GetEventsAsync_ShouldReturnEventsForAggregate()
    {
        // Arrange
        var aggregateId = "family-1";
        var event1 = DomainEvent.Create<FamilyCreatedEvent>(aggregateId, "Family", 1, "user-1", "correlation-1") with
        {
            FamilyName = "Test Family",
            CreatedBy = "user-1"
        };
        var event2 = DomainEvent.Create<FamilyMemberAddedEvent>(aggregateId, "Family", 2, "user-1", "correlation-2") with
        {
            MemberId = "member-1",
            MemberName = "John Doe",
            MemberEmail = "john@example.com",
            Role = "Child",
            AddedBy = "user-1"
        };

        await _eventStore.SaveEventAsync(event1);
        await _eventStore.SaveEventAsync(event2);

        // Act
        var events = await _eventStore.GetEventsAsync(aggregateId);

        // Assert
        events.Should().HaveCount(2);
        events.Should().BeInAscendingOrder(e => e.Version);
        events.First().Should().BeOfType<FamilyCreatedEvent>();
        events.Last().Should().BeOfType<FamilyMemberAddedEvent>();
    }

    [Fact]
    public async Task GetEventsAsync_WithFromVersion_ShouldReturnEventsAfterVersion()
    {
        // Arrange
        var aggregateId = "family-1";
        var event1 = DomainEvent.Create<FamilyCreatedEvent>(aggregateId, "Family", 1, "user-1", "correlation-1") with
        {
            FamilyName = "Test Family",
            CreatedBy = "user-1"
        };
        var event2 = DomainEvent.Create<FamilyMemberAddedEvent>(aggregateId, "Family", 2, "user-1", "correlation-2") with
        {
            MemberId = "member-1",
            MemberName = "John Doe",
            MemberEmail = "john@example.com",
            Role = "Child",
            AddedBy = "user-1"
        };

        await _eventStore.SaveEventAsync(event1);
        await _eventStore.SaveEventAsync(event2);

        // Act
        var events = await _eventStore.GetEventsAsync(aggregateId, fromVersion: 1);

        // Assert
        events.Should().HaveCount(1);
        events.First().Should().BeOfType<FamilyMemberAddedEvent>();
        events.First().Version.Should().Be(2);
    }

    [Fact]
    public async Task SaveEventsAsync_WithConcurrencyConflict_ShouldThrowException()
    {
        // Arrange
        var aggregateId = "family-1";
        var existingEvent = DomainEvent.Create<FamilyCreatedEvent>(aggregateId, "Family", 1, "user-1", "correlation-1") with
        {
            FamilyName = "Test Family",
            CreatedBy = "user-1"
        };
        await _eventStore.SaveEventAsync(existingEvent);

        var newEvents = new[]
        {
            DomainEvent.Create<FamilyMemberAddedEvent>(aggregateId, "Family", 2, "user-1", "correlation-2") with
            {
                MemberId = "member-1",
                MemberName = "John Doe",
                MemberEmail = "john@example.com",
                Role = "Child",
                AddedBy = "user-1"
            }
        };

        // Act & Assert
        var action = async () => await _eventStore.SaveEventsAsync(aggregateId, newEvents, expectedVersion: 0);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Concurrency conflict: expected version 0, but current version is 1");
    }

    [Fact]
    public async Task GetEventsByTypeAsync_ShouldReturnEventsOfSpecificType()
    {
        // Arrange
        var event1 = DomainEvent.Create<FamilyCreatedEvent>("family-1", "Family", 1, "user-1", "correlation-1") with
        {
            FamilyName = "Test Family 1",
            CreatedBy = "user-1"
        };
        var event2 = DomainEvent.Create<FamilyCreatedEvent>("family-2", "Family", 1, "user-2", "correlation-2") with
        {
            FamilyName = "Test Family 2",
            CreatedBy = "user-2"
        };
        var event3 = DomainEvent.Create<FamilyMemberAddedEvent>("family-1", "Family", 2, "user-1", "correlation-3") with
        {
            MemberId = "member-1",
            MemberName = "John Doe",
            MemberEmail = "john@example.com",
            Role = "Child",
            AddedBy = "user-1"
        };

        await _eventStore.SaveEventAsync(event1);
        await _eventStore.SaveEventAsync(event2);
        await _eventStore.SaveEventAsync(event3);

        // Act
        var events = await _eventStore.GetEventsByTypeAsync(nameof(FamilyCreatedEvent));

        // Assert
        events.Should().HaveCount(2);
        events.Should().AllBeOfType<FamilyCreatedEvent>();
        events.Should().BeInDescendingOrder(e => e.Timestamp);
    }

    [Fact]
    public async Task GetLatestVersionAsync_ShouldReturnHighestVersion()
    {
        // Arrange
        var aggregateId = "family-1";
        var event1 = DomainEvent.Create<FamilyCreatedEvent>(aggregateId, "Family", 1, "user-1", "correlation-1") with
        {
            FamilyName = "Test Family",
            CreatedBy = "user-1"
        };
        var event2 = DomainEvent.Create<FamilyMemberAddedEvent>(aggregateId, "Family", 2, "user-1", "correlation-2") with
        {
            MemberId = "member-1",
            MemberName = "John Doe",
            MemberEmail = "john@example.com",
            Role = "Child",
            AddedBy = "user-1"
        };

        await _eventStore.SaveEventAsync(event1);
        await _eventStore.SaveEventAsync(event2);

        // Act
        var latestVersion = await _eventStore.GetLatestVersionAsync(aggregateId);

        // Assert
        latestVersion.Should().Be(2);
    }

    [Fact]
    public async Task GetLatestVersionAsync_WithNoEvents_ShouldReturnZero()
    {
        // Act
        var latestVersion = await _eventStore.GetLatestVersionAsync("non-existent-aggregate");

        // Assert
        latestVersion.Should().Be(0);
    }

    [Fact]
    public async Task EventExistsAsync_WithExistingEvent_ShouldReturnTrue()
    {
        // Arrange
        var domainEvent = DomainEvent.Create<FamilyCreatedEvent>(
            "family-1", "Family", 1, "user-1", "correlation-1") with
        {
            FamilyName = "Test Family",
            CreatedBy = "user-1"
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