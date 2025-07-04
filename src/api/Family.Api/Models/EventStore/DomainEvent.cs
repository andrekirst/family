using System.ComponentModel.DataAnnotations;

namespace Family.Api.Models.EventStore;

public abstract record DomainEvent
{
    [Required]
    public string EventId { get; init; } = Guid.NewGuid().ToString();
    
    [Required]
    public string AggregateId { get; init; } = string.Empty;
    
    [Required]
    public string AggregateType { get; init; } = string.Empty;
    
    [Required]
    public string EventType { get; init; } = string.Empty;
    
    [Required]
    public int Version { get; init; }
    
    [Required]
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    
    [Required]
    public string UserId { get; init; } = string.Empty;
    
    [Required]
    public string CorrelationId { get; init; } = string.Empty;
    
    [Required]
    public string CausationId { get; init; } = string.Empty;
    
    public Dictionary<string, object> Metadata { get; init; } = new();
    
    public static T Create<T>(string aggregateId, string aggregateType, int version, 
        string userId, string correlationId, string? causationId = null, 
        Dictionary<string, object>? metadata = null) where T : DomainEvent, new()
    {
        var eventType = typeof(T).Name;
        var eventId = Guid.NewGuid().ToString();
        
        return new T
        {
            EventId = eventId,
            AggregateId = aggregateId,
            AggregateType = aggregateType,
            EventType = eventType,
            Version = version,
            Timestamp = DateTime.UtcNow,
            UserId = userId,
            CorrelationId = correlationId,
            CausationId = causationId ?? eventId,
            Metadata = metadata ?? new Dictionary<string, object>()
        };
    }
}