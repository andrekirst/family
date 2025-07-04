using Family.Infrastructure.EventSourcing.Models;

namespace Family.Api.Features.Families.DomainEvents;

public record FamilyMemberAdded : DomainEvent
{
    public Guid MemberUserId { get; init; }
    public string Role { get; init; } = string.Empty;
    public DateTime JoinedAt { get; init; }
    
    public static FamilyMemberAdded Create(
        string familyId,
        Guid userId,
        string role,
        int version,
        string requestUserId,
        string correlationId,
        string? causationId = null)
    {
        return new FamilyMemberAdded
        {
            EventId = Guid.NewGuid().ToString(),
            AggregateId = familyId,
            AggregateType = nameof(Models.Family),
            EventType = nameof(FamilyMemberAdded),
            Version = version,
            Timestamp = DateTime.UtcNow,
            UserId = requestUserId,
            CorrelationId = correlationId,
            CausationId = causationId ?? Guid.NewGuid().ToString(),
            Metadata = new Dictionary<string, object>
            {
                ["MemberUserId"] = userId.ToString(),
                ["Role"] = role,
                ["JoinedAt"] = DateTime.UtcNow.ToString("O")
            },
            MemberUserId = userId,
            Role = role,
            JoinedAt = DateTime.UtcNow
        };
    }
}