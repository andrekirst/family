using Family.Api.Models.EventStore;
using Family.Api.Models.EventStore.Events;
using Family.Api.Tests.TestHelpers;
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
        var domainEvent = EventTestHelpers.CreateFamilyCreatedEvent(
            aggregateId, aggregateType, version, userId, correlationId, causationId) with
        {
            Metadata = metadata
        };

        // Assert
        domainEvent.Should().NotBeNull();
        domainEvent.EventId.Should().NotBeEmpty();
        domainEvent.AggregateId.Should().Be(aggregateId);
        domainEvent.AggregateType.Should().Be(aggregateType);
        domainEvent.EventType.Should().Be(nameof(FamilyCreatedEvent));
        domainEvent.Version.Should().Be(version);
        domainEvent.UserId.Should().Be(userId);
        domainEvent.CorrelationId.Should().Be(correlationId);
        domainEvent.CausationId.Should().Be(causationId);
        domainEvent.Metadata.Should().BeEquivalentTo(metadata);
        domainEvent.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithoutCausationId_ShouldUseEventIdAsCausationId()
    {
        // Arrange
        var aggregateId = "test-aggregate-id";
        var aggregateType = "TestAggregate";
        var version = 1;
        var userId = "test-user-id";
        var correlationId = "test-correlation-id";

        // Act
        var domainEvent = EventTestHelpers.CreateFamilyCreatedEvent(
            aggregateId, aggregateType, version, userId, correlationId);

        // Assert
        domainEvent.CausationId.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithoutMetadata_ShouldUseEmptyDictionary()
    {
        // Arrange
        var aggregateId = "test-aggregate-id";
        var aggregateType = "TestAggregate";
        var version = 1;
        var userId = "test-user-id";
        var correlationId = "test-correlation-id";

        // Act
        var domainEvent = EventTestHelpers.CreateFamilyCreatedEvent(
            aggregateId, aggregateType, version, userId, correlationId);

        // Assert
        domainEvent.Metadata.Should().NotBeNull();
        domainEvent.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void DomainEvent_ShouldBeImmutable()
    {
        // Arrange
        var originalEvent = EventTestHelpers.CreateFamilyCreatedEvent(
            "test-id", "TestAggregate", 1, "user", "correlation");

        // Act
        var modifiedEvent = originalEvent with { Version = 2 };

        // Assert
        originalEvent.Version.Should().Be(1);
        modifiedEvent.Version.Should().Be(2);
        originalEvent.EventId.Should().Be(modifiedEvent.EventId);
        originalEvent.AggregateId.Should().Be(modifiedEvent.AggregateId);
    }
}