using Family.Api.Models.EventStore;

namespace Family.Api.Models.Base;

public abstract class AggregateRoot
{
    private readonly List<DomainEvent> _uncommittedEvents = new();
    
    public string Id { get; protected set; } = string.Empty;
    public int Version { get; protected set; } = 0;
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;
    
    public IReadOnlyList<DomainEvent> UncommittedEvents => _uncommittedEvents.AsReadOnly();
    
    public void MarkEventsAsCommitted()
    {
        _uncommittedEvents.Clear();
    }
    
    public void LoadFromHistory(IEnumerable<DomainEvent> events)
    {
        foreach (var domainEvent in events)
        {
            ApplyEvent(domainEvent, isNew: false);
        }
    }
    
    public void ReplayEvents(IEnumerable<DomainEvent> events)
    {
        Version = 0;
        _uncommittedEvents.Clear();
        
        foreach (var domainEvent in events)
        {
            ApplyEvent(domainEvent, isNew: false);
        }
    }
    
    protected void ApplyEvent(DomainEvent domainEvent, bool isNew = true)
    {
        var eventType = domainEvent.GetType();
        var applyMethod = GetType().GetMethod("Apply", new[] { eventType });
        
        if (applyMethod == null)
        {
            throw new InvalidOperationException($"No Apply method found for event type {eventType.Name} in aggregate {GetType().Name}");
        }
        
        applyMethod.Invoke(this, new object[] { domainEvent });
        
        if (isNew)
        {
            _uncommittedEvents.Add(domainEvent);
        }
        
        Version = domainEvent.Version;
        UpdatedAt = domainEvent.Timestamp;
    }
    
    protected void RaiseEvent(DomainEvent domainEvent)
    {
        var eventWithVersion = domainEvent with { Version = Version + 1 };
        ApplyEvent(eventWithVersion);
    }
    
    protected T CreateEvent<T>(string userId, string correlationId, string? causationId = null, 
        Dictionary<string, object>? metadata = null) where T : DomainEvent
    {
        return DomainEvent.CreateBase<T>(
            aggregateId: Id,
            aggregateType: GetType().Name,
            version: Version + 1,
            userId: userId,
            correlationId: correlationId,
            causationId: causationId,
            metadata: metadata
        );
    }
}