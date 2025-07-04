using Family.Api.Models.EventStore;
using Family.Api.Models.EventStore.Events;
using Family.Api.Services.EventStore;
using Family.Api.Tests.Models.Base;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Family.Api.Tests.Services.EventStore;

public class EventReplayServiceTests
{
    private readonly IEventStore _eventStore;
    private readonly ILogger<EventReplayService> _logger;
    private readonly EventReplayService _eventReplayService;

    public EventReplayServiceTests()
    {
        _eventStore = Substitute.For<IEventStore>();
        _logger = Substitute.For<ILogger<EventReplayService>>();
        _eventReplayService = new EventReplayService(_eventStore, _logger);
    }

    [Fact]
    public async Task ReplayAggregateAsync_ShouldReplayAllEvents()
    {
        // Arrange
        var aggregateId = "family-1";
        var events = new List<DomainEvent>
        {
            DomainEvent.Create<FamilyCreatedEvent>(aggregateId, "TestAggregate", 1, "user-1", "correlation-1") with
            {
                FamilyName = "Test Family",
                CreatedBy = "user-1"
            },
            DomainEvent.Create<FamilyMemberAddedEvent>(aggregateId, "TestAggregate", 2, "user-1", "correlation-2") with
            {
                MemberId = "member-1",
                MemberName = "John Doe",
                MemberEmail = "john@example.com",
                Role = "Child",
                AddedBy = "user-1"
            }
        };

        _eventStore.GetEventsAsync(aggregateId, 0, Arg.Any<CancellationToken>())
            .Returns(events);

        // Act
        var aggregate = await _eventReplayService.ReplayAggregateAsync<TestAggregate>(aggregateId);

        // Assert
        aggregate.Should().NotBeNull();
        aggregate.Id.Should().Be(aggregateId);
        aggregate.Name.Should().Be("Test Family");
        aggregate.MemberCount.Should().Be(1);
        aggregate.Version.Should().Be(2);
        aggregate.UncommittedEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task ReplayAggregateAsync_WithNoEvents_ShouldThrowException()
    {
        // Arrange
        var aggregateId = "non-existent-family";
        _eventStore.GetEventsAsync(aggregateId, 0, Arg.Any<CancellationToken>())
            .Returns(new List<DomainEvent>());

        // Act & Assert
        var action = async () => await _eventReplayService.ReplayAggregateAsync<TestAggregate>(aggregateId);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"No events found for aggregate {aggregateId}");
    }

    [Fact]
    public async Task ReplayAggregateToVersionAsync_ShouldReplayEventsUpToSpecificVersion()
    {
        // Arrange
        var aggregateId = "family-1";
        var events = new List<DomainEvent>
        {
            DomainEvent.Create<FamilyCreatedEvent>(aggregateId, "TestAggregate", 1, "user-1", "correlation-1") with
            {
                FamilyName = "Test Family",
                CreatedBy = "user-1"
            },
            DomainEvent.Create<FamilyMemberAddedEvent>(aggregateId, "TestAggregate", 2, "user-1", "correlation-2") with
            {
                MemberId = "member-1",
                MemberName = "John Doe",
                MemberEmail = "john@example.com",
                Role = "Child",
                AddedBy = "user-1"
            },
            DomainEvent.Create<FamilyMemberAddedEvent>(aggregateId, "TestAggregate", 3, "user-1", "correlation-3") with
            {
                MemberId = "member-2",
                MemberName = "Jane Doe",
                MemberEmail = "jane@example.com",
                Role = "Child",
                AddedBy = "user-1"
            }
        };

        _eventStore.GetEventsAsync(aggregateId, 0, Arg.Any<CancellationToken>())
            .Returns(events);

        // Act
        var aggregate = await _eventReplayService.ReplayAggregateToVersionAsync<TestAggregate>(aggregateId, 2);

        // Assert
        aggregate.Should().NotBeNull();
        aggregate.Id.Should().Be(aggregateId);
        aggregate.Name.Should().Be("Test Family");
        aggregate.MemberCount.Should().Be(1); // Only first member added
        aggregate.Version.Should().Be(2);
    }

    [Fact]
    public async Task ReplayAggregateToTimestampAsync_ShouldReplayEventsUpToSpecificTimestamp()
    {
        // Arrange
        var aggregateId = "family-1";
        var baseTime = DateTime.UtcNow.AddHours(-2);
        var cutoffTime = baseTime.AddHours(1);
        
        var events = new List<DomainEvent>
        {
            DomainEvent.Create<FamilyCreatedEvent>(aggregateId, "TestAggregate", 1, "user-1", "correlation-1") with
            {
                FamilyName = "Test Family",
                CreatedBy = "user-1",
                Timestamp = baseTime
            },
            DomainEvent.Create<FamilyMemberAddedEvent>(aggregateId, "TestAggregate", 2, "user-1", "correlation-2") with
            {
                MemberId = "member-1",
                MemberName = "John Doe",
                MemberEmail = "john@example.com",
                Role = "Child",
                AddedBy = "user-1",
                Timestamp = baseTime.AddMinutes(30)
            },
            DomainEvent.Create<FamilyMemberAddedEvent>(aggregateId, "TestAggregate", 3, "user-1", "correlation-3") with
            {
                MemberId = "member-2",
                MemberName = "Jane Doe",
                MemberEmail = "jane@example.com",
                Role = "Child",
                AddedBy = "user-1",
                Timestamp = baseTime.AddHours(2) // After cutoff
            }
        };

        _eventStore.GetEventsAsync(aggregateId, 0, Arg.Any<CancellationToken>())
            .Returns(events);

        // Act
        var aggregate = await _eventReplayService.ReplayAggregateToTimestampAsync<TestAggregate>(aggregateId, cutoffTime);

        // Assert
        aggregate.Should().NotBeNull();
        aggregate.Id.Should().Be(aggregateId);
        aggregate.Name.Should().Be("Test Family");
        aggregate.MemberCount.Should().Be(1); // Only first member added before cutoff
        aggregate.Version.Should().Be(2);
    }

    [Fact]
    public async Task GetEventHistoryAsync_ShouldReturnAllEventsForAggregate()
    {
        // Arrange
        var aggregateId = "family-1";
        var events = new List<DomainEvent>
        {
            DomainEvent.Create<FamilyCreatedEvent>(aggregateId, "TestAggregate", 1, "user-1", "correlation-1"),
            DomainEvent.Create<FamilyMemberAddedEvent>(aggregateId, "TestAggregate", 2, "user-1", "correlation-2")
        };

        _eventStore.GetEventsAsync(aggregateId, 0, Arg.Any<CancellationToken>())
            .Returns(events);

        // Act
        var result = await _eventReplayService.GetEventHistoryAsync(aggregateId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(events);
    }

    [Fact]
    public async Task GetEventHistoryAsync_WithVersionRange_ShouldReturnFilteredEvents()
    {
        // Arrange
        var aggregateId = "family-1";
        var events = new List<DomainEvent>
        {
            DomainEvent.Create<FamilyCreatedEvent>(aggregateId, "TestAggregate", 1, "user-1", "correlation-1"),
            DomainEvent.Create<FamilyMemberAddedEvent>(aggregateId, "TestAggregate", 2, "user-1", "correlation-2"),
            DomainEvent.Create<FamilyMemberAddedEvent>(aggregateId, "TestAggregate", 3, "user-1", "correlation-3"),
            DomainEvent.Create<FamilyMemberRemovedEvent>(aggregateId, "TestAggregate", 4, "user-1", "correlation-4")
        };

        _eventStore.GetEventsAsync(aggregateId, 0, Arg.Any<CancellationToken>())
            .Returns(events);

        // Act
        var result = await _eventReplayService.GetEventHistoryAsync(aggregateId, fromVersion: 2, toVersion: 3);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(e => e.Version.Should().BeInRange(2, 3));
    }

    [Fact]
    public async Task ValidateEventSequenceAsync_WithValidSequence_ShouldReturnTrue()
    {
        // Arrange
        var aggregateId = "family-1";
        var events = new List<DomainEvent>
        {
            DomainEvent.Create<FamilyCreatedEvent>(aggregateId, "TestAggregate", 1, "user-1", "correlation-1"),
            DomainEvent.Create<FamilyMemberAddedEvent>(aggregateId, "TestAggregate", 2, "user-1", "correlation-2"),
            DomainEvent.Create<FamilyMemberAddedEvent>(aggregateId, "TestAggregate", 3, "user-1", "correlation-3")
        };

        _eventStore.GetEventsAsync(aggregateId, 0, Arg.Any<CancellationToken>())
            .Returns(events);

        // Act
        var isValid = await _eventReplayService.ValidateEventSequenceAsync(aggregateId);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateEventSequenceAsync_WithInvalidSequence_ShouldReturnFalse()
    {
        // Arrange
        var aggregateId = "family-1";
        var events = new List<DomainEvent>
        {
            DomainEvent.Create<FamilyCreatedEvent>(aggregateId, "TestAggregate", 1, "user-1", "correlation-1"),
            DomainEvent.Create<FamilyMemberAddedEvent>(aggregateId, "TestAggregate", 3, "user-1", "correlation-2"), // Missing version 2
            DomainEvent.Create<FamilyMemberAddedEvent>(aggregateId, "TestAggregate", 4, "user-1", "correlation-3")
        };

        _eventStore.GetEventsAsync(aggregateId, 0, Arg.Any<CancellationToken>())
            .Returns(events);

        // Act
        var isValid = await _eventReplayService.ValidateEventSequenceAsync(aggregateId);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public async Task GetAggregateStateAtVersionAsync_ShouldReturnStateAtSpecificVersion()
    {
        // Arrange
        var aggregateId = "family-1";
        var events = new List<DomainEvent>
        {
            DomainEvent.Create<FamilyCreatedEvent>(aggregateId, "TestAggregate", 1, "user-1", "correlation-1") with
            {
                FamilyName = "Test Family",
                CreatedBy = "user-1"
            },
            DomainEvent.Create<FamilyMemberAddedEvent>(aggregateId, "TestAggregate", 2, "user-1", "correlation-2") with
            {
                MemberId = "member-1",
                MemberName = "John Doe",
                MemberEmail = "john@example.com",
                Role = "Child",
                AddedBy = "user-1"
            }
        };

        _eventStore.GetEventsAsync(aggregateId, 0, Arg.Any<CancellationToken>())
            .Returns(events);

        // Act
        var state = await _eventReplayService.GetAggregateStateAtVersionAsync<TestAggregate>(aggregateId, 1);

        // Assert
        state.Should().ContainKey("Id");
        state.Should().ContainKey("Version");
        state.Should().ContainKey("State");
        state["Id"].Should().Be(aggregateId);
        state["Version"].Should().Be(1);
    }
}