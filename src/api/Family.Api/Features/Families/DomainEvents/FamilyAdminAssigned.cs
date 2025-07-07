using Family.Infrastructure.EventSourcing.Models;
using MediatR;

namespace Family.Api.Features.Families.DomainEvents;

public record FamilyAdminAssigned : DomainEvent, INotification
{
    public Guid AdminUserId { get; init; }
    public DateTime AssignedAt { get; init; }
    
    public static FamilyAdminAssigned Create(
        string familyId,
        Guid adminUserId,
        int version,
        string requestUserId,
        string correlationId,
        string? causationId = null)
    {
        return new FamilyAdminAssigned
        {
            EventId = Guid.NewGuid().ToString(),
            AggregateId = familyId,
            AggregateType = nameof(Models.Family),
            EventType = nameof(FamilyAdminAssigned),
            Version = version,
            Timestamp = DateTime.UtcNow,
            UserId = requestUserId,
            CorrelationId = correlationId,
            CausationId = causationId ?? Guid.NewGuid().ToString(),
            Metadata = new Dictionary<string, object>
            {
                ["AdminUserId"] = adminUserId.ToString(),
                ["AssignedAt"] = DateTime.UtcNow.ToString("O")
            },
            AdminUserId = adminUserId,
            AssignedAt = DateTime.UtcNow
        };
    }
}