using Family.Api.Models.EventStore;
using FluentAssertions;

namespace Family.Api.Tests.Models.EventStore;

public class DomainEventTests
{
    [Fact]
    public void Create_ShouldCreateEventWithCorrectProperties()
    {
        // Arrange
        var aggregateId = "test-aggregate-id";
        var aggregateType = "TestAggregate";
        var version = 1;
        var userId = "test-user-id";
        var correlationId = "test-correlation-id";
        var causationId = "test-causation-id";
        var metadata = new Dictionary<string, object> { { "key", "value" } };

        // Act
        var domainEvent = new TestDomainEvent
        {
            EventId = Guid.NewGuid().ToString(),
            AggregateId = aggregateId,
            AggregateType = aggregateType,
            EventType = nameof(TestDomainEvent),
            Version = version,
            Timestamp = DateTime.UtcNow,
            UserId = userId,
            CorrelationId = correlationId,
            CausationId = causationId,
            Metadata = metadata,
            TestData = "test"
        };

        // Assert
        domainEvent.Should().NotBeNull();
        domainEvent.EventId.Should().NotBeEmpty();
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.AggregateType.Should().Be(aggregateType);
        domainEvent.EventType.Should().Be(nameof(TestDomainEvent));
        domainEvent.Version.Should().Be(version);
        domainEvent.UserId.Should().Be(userId);
        domainEvent.CorrelationId.Should().Be(correlationId);
        domainEvent.CausationId.Should().Be(causationId);
        domainEvent.Metadata.Should().BeEquivalentTo(metadata);
        domainEvent.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void DomainEvent_ShouldBeImmutable()
    {
        // Arrange
        var originalEvent = new TestDomainEvent
        {
            EventId = Guid.NewGuid().ToString(),
            AggregateId = "test-id",
            AggregateType = "TestAggregate",
            EventType = nameof(TestDomainEvent),
            Version = 1,
            Timestamp = DateTime.UtcNow,
            UserId = "user",
            CorrelationId = "correlation",
            CausationId = Guid.NewGuid().ToString(),
            Metadata = new Dictionary<string, object>(),
            TestData = "original"
        };

        // Act
        var modifiedEvent = originalEvent with { Version = 2, TestData = "modified" };

        // Assert
        originalEvent.Version.Should().Be(1);
        originalEvent.TestData.Should().Be("original");
        modifiedEvent.Version.Should().Be(2);
        modifiedEvent.TestData.Should().Be("modified");
        originalEvent.EventId.Should().Be(modifiedEvent.EventId);
        originalEvent.AggregateId.Should().Be(modifiedEvent.AggregateId);
    }
}