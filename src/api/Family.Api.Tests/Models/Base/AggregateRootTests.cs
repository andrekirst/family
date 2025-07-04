using Family.Api.Models.Base;
using Family.Api.Models.EventStore;
using Family.Api.Models.EventStore.Events;
using FluentAssertions;

namespace Family.Api.Tests.Models.Base;

public class TestAggregate : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public int MemberCount { get; private set; } = 0;

    public TestAggregate()
    {
    }

    public TestAggregate(string id, string name, string userId, string correlationId)
    {
        Id = id;
        var createdEvent = CreateEvent<FamilyCreatedEvent>(userId, correlationId);
        var eventWithData = createdEvent with 
        { 
            FamilyName = name,
            CreatedBy = userId
        };
        RaiseEvent(eventWithData);
    }

    public void AddMember(string memberId, string memberName, string memberEmail, 
        string role, string userId, string correlationId)
    {
        var memberAddedEvent = CreateEvent<FamilyMemberAddedEvent>(userId, correlationId);
        var eventWithData = memberAddedEvent with
        {
            MemberId = memberId,
            MemberName = memberName,
            MemberEmail = memberEmail,
            Role = role,
            AddedBy = userId
        };
        RaiseEvent(eventWithData);
    }

    public void Apply(FamilyCreatedEvent familyCreatedEvent)
    {
        Id = familyCreatedEvent.AggregateId;
        Name = familyCreatedEvent.FamilyName;
        CreatedAt = familyCreatedEvent.Timestamp;
    }

    public void Apply(FamilyMemberAddedEvent familyMemberAddedEvent)
    {
        MemberCount++;
    }
}

public class AggregateRootTests
{
    [Fact]
    public void CreateAggregate_ShouldRaiseCreatedEvent()
    {
        // Arrange
        var aggregateId = "test-family-1";
        var familyName = "Test Family";
        var userId = "user-1";
        var correlationId = "correlation-1";

        // Act
        var aggregate = new TestAggregate(aggregateId, familyName, userId, correlationId);

        // Assert
        aggregate.Id.Should().Be(aggregateId);
        aggregate.Name.Should().Be(familyName);
        aggregate.Version.Should().Be(1);
        aggregate.UncommittedEvents.Should().HaveCount(1);
        
        var createdEvent = aggregate.UncommittedEvents.First() as FamilyCreatedEvent;
        createdEvent.Should().NotBeNull();
        createdEvent!.FamilyName.Should().Be(familyName);
        createdEvent.CreatedBy.Should().Be(userId);
    }

    [Fact]
    public void AddMember_ShouldRaiseMemberAddedEvent()
    {
        // Arrange
        var aggregate = new TestAggregate("family-1", "Test Family", "user-1", "correlation-1");
        aggregate.MarkEventsAsCommitted(); // Clear initial events

        // Act
        aggregate.AddMember("member-1", "John Doe", "john@example.com", "Child", "user-1", "correlation-2");

        // Assert
        aggregate.MemberCount.Should().Be(1);
        aggregate.Version.Should().Be(2);
        aggregate.UncommittedEvents.Should().HaveCount(1);
        
        var memberAddedEvent = aggregate.UncommittedEvents.First() as FamilyMemberAddedEvent;
        memberAddedEvent.Should().NotBeNull();
        memberAddedEvent!.MemberName.Should().Be("John Doe");
        memberAddedEvent.MemberEmail.Should().Be("john@example.com");
    }

    [Fact]
    public void LoadFromHistory_ShouldReplayEventsCorrectly()
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

        // Act
        var aggregate = new TestAggregate();
        aggregate.LoadFromHistory(events);

        // Assert
        aggregate.Id.Should().Be(aggregateId);
        aggregate.Name.Should().Be("Test Family");
        aggregate.MemberCount.Should().Be(1);
        aggregate.Version.Should().Be(2);
        aggregate.UncommittedEvents.Should().BeEmpty();
    }

    [Fact]
    public void ReplayEvents_ShouldResetVersionAndReplayFromStart()
    {
        // Arrange
        var aggregate = new TestAggregate("family-1", "Test Family", "user-1", "correlation-1");
        aggregate.AddMember("member-1", "John Doe", "john@example.com", "Child", "user-1", "correlation-2");
        var currentVersion = aggregate.Version;

        var events = new List<DomainEvent>
        {
            DomainEvent.Create<FamilyCreatedEvent>("family-1", "TestAggregate", 1, "user-1", "correlation-1") with
            {
                FamilyName = "Replayed Family",
                CreatedBy = "user-1"
            }
        };

        // Act
        aggregate.ReplayEvents(events);

        // Assert
        aggregate.Name.Should().Be("Replayed Family");
        aggregate.MemberCount.Should().Be(0);
        aggregate.Version.Should().Be(1);
        aggregate.UncommittedEvents.Should().BeEmpty();
    }

    [Fact]
    public void MarkEventsAsCommitted_ShouldClearUncommittedEvents()
    {
        // Arrange
        var aggregate = new TestAggregate("family-1", "Test Family", "user-1", "correlation-1");
        
        // Act
        aggregate.MarkEventsAsCommitted();

        // Assert
        aggregate.UncommittedEvents.Should().BeEmpty();
    }

    [Fact]
    public void ApplyEvent_WithUnknownEventType_ShouldThrowException()
    {
        // Arrange
        var aggregate = new TestAggregate();
        var unknownEvent = DomainEvent.Create<SystemErrorEvent>("family-1", "TestAggregate", 1, "user-1", "correlation-1");

        // Act & Assert
        var action = () => aggregate.LoadFromHistory(new[] { unknownEvent });
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("No Apply method found for event type * in aggregate TestAggregate");
    }

    [Fact]
    public void CreateEvent_ShouldCreateEventWithCorrectAggregateInfo()
    {
        // Arrange
        var aggregate = new TestAggregate("family-1", "Test Family", "user-1", "correlation-1");
        
        // Act
        var newEvent = aggregate.CreateEvent<FamilyMemberAddedEvent>("user-2", "correlation-2", "causation-1");

        // Assert
        newEvent.AggregateId.Should().Be("family-1");
        newEvent.AggregateType.Should().Be("TestAggregate");
        newEvent.Version.Should().Be(2); // Next version after creation
        newEvent.UserId.Should().Be("user-2");
        newEvent.CorrelationId.Should().Be("correlation-2");
        newEvent.CausationId.Should().Be("causation-1");
    }
}