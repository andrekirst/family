namespace Family.Infrastructure.Messaging.Models;

public record EventEnvelope
{
    [Required]
    public string EventId { get; init; } = string.Empty;
    
    [Required]
    public string EventType { get; init; } = string.Empty;
    
    [Required]
    public string AggregateId { get; init; } = string.Empty;
    
    [Required]
    public string AggregateType { get; init; } = string.Empty;
    
    [Required]
    public int Version { get; init; }
    
    [Required]
    public DateTime Timestamp { get; init; }
    
    [Required]
    public string UserId { get; init; } = string.Empty;
    
    [Required]
    public string CorrelationId { get; init; } = string.Empty;
    
    [Required]
    public string CausationId { get; init; } = string.Empty;
    
    [Required]
    public string Data { get; init; } = string.Empty;
    
    public Dictionary<string, object> Metadata { get; init; } = new();
    
    public string? Topic { get; init; }
    
    public string? Key { get; init; }
    
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}