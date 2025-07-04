using Family.Api.Models.EventStore.Events;

namespace Family.Api.Tests.TestHelpers;

public static class EventTestHelpers
{
    public static FamilyCreatedEvent CreateFamilyCreatedEvent(
        string aggregateId = "family-1",
        string aggregateType = "Family",
        int version = 1,
        string userId = "user-1",
        string correlationId = "correlation-1",
        string? causationId = null,
        string familyName = "Test Family",
        string? createdBy = null)
    {
        return new FamilyCreatedEvent
        {
            EventId = Guid.NewGuid().ToString(),
            AggregateId = aggregateId,
            AggregateType = aggregateType,
            EventType = nameof(FamilyCreatedEvent),
            Version = version,
            Timestamp = DateTime.UtcNow,
            UserId = userId,
            CorrelationId = correlationId,
            CausationId = causationId ?? Guid.NewGuid().ToString(),
            Metadata = new Dictionary<string, object>(),
            FamilyName = familyName,
            CreatedBy = createdBy ?? userId
        };
    }

    public static FamilyMemberAddedEvent CreateFamilyMemberAddedEvent(
        string aggregateId = "family-1",
        string aggregateType = "Family",
        int version = 2,
        string userId = "user-1",
        string correlationId = "correlation-1",
        string? causationId = null,
        string memberId = "member-1",
        string memberName = "John Doe",
        string memberEmail = "john@example.com",
        string role = "Child",
        string? addedBy = null)
    {
        return new FamilyMemberAddedEvent
        {
            EventId = Guid.NewGuid().ToString(),
            AggregateId = aggregateId,
            AggregateType = aggregateType,
            EventType = nameof(FamilyMemberAddedEvent),
            Version = version,
            Timestamp = DateTime.UtcNow,
            UserId = userId,
            CorrelationId = correlationId,
            CausationId = causationId ?? Guid.NewGuid().ToString(),
            Metadata = new Dictionary<string, object>(),
            MemberId = memberId,
            MemberName = memberName,
            MemberEmail = memberEmail,
            Role = role,
            AddedBy = addedBy ?? userId
        };
    }

    public static FamilyMemberRemovedEvent CreateFamilyMemberRemovedEvent(
        string aggregateId = "family-1",
        string aggregateType = "Family",
        int version = 3,
        string userId = "user-1",
        string correlationId = "correlation-1",
        string? causationId = null,
        string memberId = "member-1",
        string memberName = "John Doe",
        string? removedBy = null,
        string? reason = null)
    {
        return new FamilyMemberRemovedEvent
        {
            EventId = Guid.NewGuid().ToString(),
            AggregateId = aggregateId,
            AggregateType = aggregateType,
            EventType = nameof(FamilyMemberRemovedEvent),
            Version = version,
            Timestamp = DateTime.UtcNow,
            UserId = userId,
            CorrelationId = correlationId,
            CausationId = causationId ?? Guid.NewGuid().ToString(),
            Metadata = new Dictionary<string, object>(),
            MemberId = memberId,
            MemberName = memberName,
            RemovedBy = removedBy ?? userId,
            Reason = reason
        };
    }
}