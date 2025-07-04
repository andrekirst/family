using Family.Infrastructure.EventSourcing.Models;
using MediatR;

namespace Family.Api.Features.Families.DomainEvents;

public record FamilyCreated : DomainEvent, INotification
{
    public string Name { get; init; } = string.Empty;
    public Guid OwnerId { get; init; }
    
    public static FamilyCreated Create(
        string familyId, 
        string name, 
        Guid ownerId, 
        string userId, 
        string correlationId,
        string? causationId = null)
    {
        return new FamilyCreated
        {
            EventId = Guid.NewGuid().ToString(),
            AggregateId = familyId,
            AggregateType = nameof(Models.Family),
            EventType = nameof(FamilyCreated),
            Version = 1,
            Timestamp = DateTime.UtcNow,
            UserId = userId,
            CorrelationId = correlationId,
            CausationId = causationId ?? Guid.NewGuid().ToString(),
            Metadata = new Dictionary<string, object>
            {
                ["FamilyName"] = name,
                ["OwnerId"] = ownerId.ToString()
            },
            Name = name,
            OwnerId = ownerId
        };
    }
}