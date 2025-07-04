using Family.Api.Models.Base;
using Family.Api.Models.EventStore;
using Family.Api.Tests.Models.EventStore;
using FluentAssertions;

namespace Family.Api.Tests.Models.Base;

public class TestAggregate : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string? Data { get; private set; }

    public TestAggregate()
    {
    }

    public TestAggregate(string id, string name, string userId, string correlationId)
    {
        Id = id;
        var createdEvent = new TestDomainEvent
        {
            EventId = Guid.NewGuid().ToString(),
            AggregateId = id,
            AggregateType = GetType().Name,
            EventType = nameof(TestDomainEvent),
            Version = 1,
            Timestamp = DateTime.UtcNow,
            UserId = userId,
            CorrelationId = correlationId,
            CausationId = Guid.NewGuid().ToString(),
            Metadata = new Dictionary<string, object>(),
            TestData = $"Created:{name}"
        };
        RaiseEvent(createdEvent);
    }

    public void UpdateData(string data, string userId, string correlationId)
    {
        var updatedEvent = new TestDomainEvent
        {
            EventId = Guid.NewGuid().ToString(),
            AggregateId = Id,
            AggregateType = GetType().Name,
            EventType = nameof(TestDomainEvent),
            Version = Version + 1,
            Timestamp = DateTime.UtcNow,
            UserId = userId,
            CorrelationId = correlationId,
            CausationId = Guid.NewGuid().ToString(),
            Metadata = new Dictionary<string, object>(),
            TestData = $"Updated:{data}"
        };
        RaiseEvent(updatedEvent);
    }

    public void Apply(TestDomainEvent testEvent)
    {
        if (testEvent.TestData?.StartsWith("Created:") == true)
        {
            Id = testEvent.AggregateId;
            Name = testEvent.TestData.Substring("Created:".Length);
            CreatedAt = testEvent.Timestamp;
        }
        else if (testEvent.TestData?.StartsWith("Updated:") == true)
        {
            Data = testEvent.TestData.Substring("Updated:".Length);
        }
    }
}

public class AggregateRootTests
{
    [Fact]
    public void CreateAggregate_ShouldRaiseCreatedEvent()
    {
        // Arrange
        var aggregateId = "test-aggregate-1";
        var name = "Test Name";
        var userId = "user-1";
        var correlationId = "correlation-1";

        // Act
        var aggregate = new TestAggregate(aggregateId, name, userId, correlationId);

        // Assert
        aggregate.Id.Should().Be(aggregateId);
        aggregate.Name.Should().Be(name);
        aggregate.Version.Should().Be(1);
        aggregate.UncommittedEvents.Should().HaveCount(1);
        
        var createdEvent = aggregate.UncommittedEvents.First() as TestDomainEvent;
        createdEvent.Should().NotBeNull();
        createdEvent!.TestData.Should().Be($"Created:{name}");
        createdEvent.UserId.Should().Be(userId);
    }

    [Fact]
    public void UpdateData_ShouldRaiseUpdatedEvent()
    {
        // Arrange
        var aggregate = new TestAggregate("aggregate-1", "Test Name", "user-1", "correlation-1");
        aggregate.MarkEventsAsCommitted(); // Clear initial events

        // Act
        aggregate.UpdateData("Updated Data", "user-1", "correlation-2");

        // Assert
        aggregate.Data.Should().Be("Updated Data");
        aggregate.Version.Should().Be(2);
        aggregate.UncommittedEvents.Should().HaveCount(1);
        
        var updatedEvent = aggregate.UncommittedEvents.First() as TestDomainEvent;
        updatedEvent.Should().NotBeNull();
        updatedEvent!.TestData.Should().Be("Updated:Updated Data");
        updatedEvent.UserId.Should().Be("user-1");
    }

    [Fact]
    public void LoadFromHistory_ShouldReplayEventsCorrectly()
    {
        // Arrange
        var aggregateId = "aggregate-1";
        var events = new List<DomainEvent>
        {
            new TestDomainEvent
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
                TestData = "Created:Test Name"
            },
            new TestDomainEvent
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
                TestData = "Updated:Test Data"
            }
        };

        // Act
        var aggregate = new TestAggregate();
        aggregate.LoadFromHistory(events);

        // Assert
        aggregate.Id.Should().Be(aggregateId);
        aggregate.Name.Should().Be("Test Name");
        aggregate.Data.Should().Be("Test Data");
        aggregate.Version.Should().Be(2);
        aggregate.UncommittedEvents.Should().BeEmpty();
    }

    [Fact]
    public void ReplayEvents_ShouldResetVersionAndReplayFromStart()
    {
        // Arrange
        var aggregate = new TestAggregate("aggregate-1", "Test Name", "user-1", "correlation-1");
        aggregate.UpdateData("Initial Data", "user-1", "correlation-2");
        var currentVersion = aggregate.Version;

        var events = new List<DomainEvent>
        {
            new TestDomainEvent
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
                TestData = "Created:Replayed Name"
            }
        };

        // Act
        aggregate.ReplayEvents(events);

        // Assert
        aggregate.Name.Should().Be("Replayed Name");
        aggregate.Data.Should().BeNull();
        aggregate.Version.Should().Be(1);
        aggregate.UncommittedEvents.Should().BeEmpty();
    }

    [Fact]
    public void MarkEventsAsCommitted_ShouldClearUncommittedEvents()
    {
        // Arrange
        var aggregate = new TestAggregate("aggregate-1", "Test Name", "user-1", "correlation-1");
        
        // Act
        aggregate.MarkEventsAsCommitted();

        // Assert
        aggregate.UncommittedEvents.Should().BeEmpty();
    }
}