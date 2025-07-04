using Family.Api.Models.EventStore.Events;
using FluentAssertions;

namespace Family.Api.Tests.Models.EventStore.Events;

public class FamilyEventsTests
{
    [Fact]
    public void FamilyCreatedEvent_ShouldCreateWithRequiredProperties()
    {
        // Arrange & Act
        var familyEvent = new FamilyCreatedEvent
        {
            EventId = Guid.NewGuid().ToString(),
            AggregateId = "family-1",
            AggregateType = "Family",
            EventType = nameof(FamilyCreatedEvent),
            Version = 1,
            Timestamp = DateTime.UtcNow,
            UserId = "user-1",
            CorrelationId = "correlation-1",
            CausationId = "causation-1",
            FamilyName = "Test Family",
            CreatedBy = "user-1"
        };

        // Assert
        familyEvent.Should().NotBeNull();
        familyEvent.FamilyName.Should().Be("Test Family");
        familyEvent.CreatedBy.Should().Be("user-1");
        familyEvent.AdditionalProperties.Should().NotBeNull();
        familyEvent.AdditionalProperties.Should().BeEmpty();
    }

    [Fact]
    public void FamilyMemberAddedEvent_ShouldCreateWithRequiredProperties()
    {
        // Arrange & Act
        var memberEvent = new FamilyMemberAddedEvent
        {
            EventId = Guid.NewGuid().ToString(),
            AggregateId = "family-1",
            AggregateType = "Family",
            EventType = nameof(FamilyMemberAddedEvent),
            Version = 2,
            Timestamp = DateTime.UtcNow,
            UserId = "user-1",
            CorrelationId = "correlation-1",
            CausationId = "causation-1",
            MemberId = "member-1",
            MemberName = "John Doe",
            MemberEmail = "john@example.com",
            Role = "Child",
            AddedBy = "user-1"
        };

        // Assert
        memberEvent.Should().NotBeNull();
        memberEvent.MemberId.Should().Be("member-1");
        memberEvent.MemberName.Should().Be("John Doe");
        memberEvent.MemberEmail.Should().Be("john@example.com");
        memberEvent.Role.Should().Be("Child");
        memberEvent.AddedBy.Should().Be("user-1");
    }

    [Fact]
    public void FamilyMemberRemovedEvent_ShouldCreateWithRequiredProperties()
    {
        // Arrange & Act
        var removedEvent = new FamilyMemberRemovedEvent
        {
            EventId = Guid.NewGuid().ToString(),
            AggregateId = "family-1",
            AggregateType = "Family",
            EventType = nameof(FamilyMemberRemovedEvent),
            Version = 3,
            Timestamp = DateTime.UtcNow,
            UserId = "user-1",
            CorrelationId = "correlation-1",
            CausationId = "causation-1",
            MemberId = "member-1",
            MemberName = "John Doe",
            RemovedBy = "user-1",
            Reason = "Member left family"
        };

        // Assert
        removedEvent.Should().NotBeNull();
        removedEvent.MemberId.Should().Be("member-1");
        removedEvent.MemberName.Should().Be("John Doe");
        removedEvent.RemovedBy.Should().Be("user-1");
        removedEvent.Reason.Should().Be("Member left family");
    }

    [Fact]
    public void FamilyMemberRoleChangedEvent_ShouldCreateWithRequiredProperties()
    {
        // Arrange & Act
        var roleChangeEvent = new FamilyMemberRoleChangedEvent
        {
            EventId = Guid.NewGuid().ToString(),
            AggregateId = "family-1",
            AggregateType = "Family",
            EventType = nameof(FamilyMemberRoleChangedEvent),
            Version = 4,
            Timestamp = DateTime.UtcNow,
            UserId = "user-1",
            CorrelationId = "correlation-1",
            CausationId = "causation-1",
            MemberId = "member-1",
            MemberName = "John Doe",
            OldRole = "Child",
            NewRole = "Adult",
            ChangedBy = "user-1"
        };

        // Assert
        roleChangeEvent.Should().NotBeNull();
        roleChangeEvent.MemberId.Should().Be("member-1");
        roleChangeEvent.MemberName.Should().Be("John Doe");
        roleChangeEvent.OldRole.Should().Be("Child");
        roleChangeEvent.NewRole.Should().Be("Adult");
        roleChangeEvent.ChangedBy.Should().Be("user-1");
    }
}