using Family.Api.Models.Base;
using Family.Api.Models.EventStore;

namespace Family.Api.Services.EventStore;

public interface IEventReplayService
{
    Task<T> ReplayAggregateAsync<T>(string aggregateId, CancellationToken cancellationToken = default) where T : AggregateRoot, new();
    
    Task<T> ReplayAggregateToVersionAsync<T>(string aggregateId, int version, CancellationToken cancellationToken = default) where T : AggregateRoot, new();
    
    Task<T> ReplayAggregateToTimestampAsync<T>(string aggregateId, DateTime timestamp, CancellationToken cancellationToken = default) where T : AggregateRoot, new();
    
    Task<IEnumerable<DomainEvent>> GetEventHistoryAsync(string aggregateId, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<DomainEvent>> GetEventHistoryAsync(string aggregateId, int fromVersion, int toVersion, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<DomainEvent>> GetEventHistoryAsync(string aggregateId, DateTime fromTimestamp, DateTime toTimestamp, CancellationToken cancellationToken = default);
    
    Task<bool> ValidateEventSequenceAsync(string aggregateId, CancellationToken cancellationToken = default);
    
    Task<Dictionary<string, object>> GetAggregateStateAtVersionAsync<T>(string aggregateId, int version, CancellationToken cancellationToken = default) where T : AggregateRoot, new();
    
    Task<Dictionary<string, object>> GetAggregateStateAtTimestampAsync<T>(string aggregateId, DateTime timestamp, CancellationToken cancellationToken = default) where T : AggregateRoot, new();
}