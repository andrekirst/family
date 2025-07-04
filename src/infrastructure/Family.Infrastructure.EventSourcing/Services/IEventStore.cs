using Family.Infrastructure.EventSourcing.Models;

namespace Family.Infrastructure.EventSourcing.Services;

public interface IEventStore
{
    Task<IEnumerable<DomainEvent>> GetEventsAsync(string aggregateId, int fromVersion = 0, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<DomainEvent>> GetEventsAsync(string aggregateId, DateTime fromTimestamp, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<DomainEvent>> GetEventsByTypeAsync(string eventType, int pageSize = 100, int pageNumber = 1, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<DomainEvent>> GetEventsByAggregateTypeAsync(string aggregateType, int pageSize = 100, int pageNumber = 1, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<DomainEvent>> GetEventsByUserAsync(string userId, int pageSize = 100, int pageNumber = 1, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<DomainEvent>> GetEventsByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default);
    
    Task SaveEventsAsync(string aggregateId, IEnumerable<DomainEvent> events, int expectedVersion, CancellationToken cancellationToken = default);
    
    Task SaveEventAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default);
    
    Task<Snapshot?> GetSnapshotAsync(string aggregateId, CancellationToken cancellationToken = default);
    
    Task SaveSnapshotAsync(Snapshot snapshot, CancellationToken cancellationToken = default);
    
    Task<bool> EventExistsAsync(string eventId, CancellationToken cancellationToken = default);
    
    Task<int> GetLatestVersionAsync(string aggregateId, CancellationToken cancellationToken = default);
}