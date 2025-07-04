namespace Family.Infrastructure.EventSourcing.Models;

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
}