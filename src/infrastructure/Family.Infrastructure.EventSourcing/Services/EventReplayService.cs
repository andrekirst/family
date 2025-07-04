using Family.Infrastructure.EventSourcing.Models;
using Family.Infrastructure.EventSourcing.Models;
using System.Text.Json;

namespace Family.Infrastructure.EventSourcing.Services;

public class EventReplayService : IEventReplayService
{
    private readonly IEventStore _eventStore;
    private readonly ILogger<EventReplayService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public EventReplayService(IEventStore eventStore, ILogger<EventReplayService> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public async Task<T> ReplayAggregateAsync<T>(string aggregateId, CancellationToken cancellationToken = default) where T : AggregateRoot, new()
    {
        _logger.LogDebug("Replaying aggregate {AggregateType} with ID {AggregateId}", typeof(T).Name, aggregateId);
        
        var events = await _eventStore.GetEventsAsync(aggregateId, 0, cancellationToken);
        
        if (!events.Any())
        {
            throw new InvalidOperationException($"No events found for aggregate {aggregateId}");
        }
        
        var aggregate = new T();
        aggregate.ReplayEvents(events);
        
        _logger.LogInformation("Successfully replayed {EventCount} events for aggregate {AggregateId}", 
            events.Count(), aggregateId);
        
        return aggregate;
    }

    public async Task<T> ReplayAggregateToVersionAsync<T>(string aggregateId, int version, CancellationToken cancellationToken = default) where T : AggregateRoot, new()
    {
        _logger.LogDebug("Replaying aggregate {AggregateType} with ID {AggregateId} to version {Version}", 
            typeof(T).Name, aggregateId, version);
        
        var events = await _eventStore.GetEventsAsync(aggregateId, 0, cancellationToken);
        var eventsUpToVersion = events.Where(e => e.Version <= version).ToList();
        
        if (!eventsUpToVersion.Any())
        {
            throw new InvalidOperationException($"No events found for aggregate {aggregateId} up to version {version}");
        }
        
        var aggregate = new T();
        aggregate.ReplayEvents(eventsUpToVersion);
        
        _logger.LogInformation("Successfully replayed {EventCount} events for aggregate {AggregateId} to version {Version}", 
            eventsUpToVersion.Count, aggregateId, version);
        
        return aggregate;
    }

    public async Task<T> ReplayAggregateToTimestampAsync<T>(string aggregateId, DateTime timestamp, CancellationToken cancellationToken = default) where T : AggregateRoot, new()
    {
        _logger.LogDebug("Replaying aggregate {AggregateType} with ID {AggregateId} to timestamp {Timestamp}", 
            typeof(T).Name, aggregateId, timestamp);
        
        var events = await _eventStore.GetEventsAsync(aggregateId, 0, cancellationToken);
        var eventsUpToTimestamp = events.Where(e => e.Timestamp <= timestamp).ToList();
        
        if (!eventsUpToTimestamp.Any())
        {
            throw new InvalidOperationException($"No events found for aggregate {aggregateId} up to timestamp {timestamp}");
        }
        
        var aggregate = new T();
        aggregate.ReplayEvents(eventsUpToTimestamp);
        
        _logger.LogInformation("Successfully replayed {EventCount} events for aggregate {AggregateId} to timestamp {Timestamp}", 
            eventsUpToTimestamp.Count, aggregateId, timestamp);
        
        return aggregate;
    }

    public async Task<IEnumerable<DomainEvent>> GetEventHistoryAsync(string aggregateId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting event history for aggregate {AggregateId}", aggregateId);
        
        var events = await _eventStore.GetEventsAsync(aggregateId, 0, cancellationToken);
        
        _logger.LogDebug("Found {EventCount} events for aggregate {AggregateId}", events.Count(), aggregateId);
        
        return events;
    }

    public async Task<IEnumerable<DomainEvent>> GetEventHistoryAsync(string aggregateId, int fromVersion, int toVersion, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting event history for aggregate {AggregateId} from version {FromVersion} to {ToVersion}", 
            aggregateId, fromVersion, toVersion);
        
        var events = await _eventStore.GetEventsAsync(aggregateId, 0, cancellationToken);
        var filteredEvents = events.Where(e => e.Version >= fromVersion && e.Version <= toVersion).ToList();
        
        _logger.LogDebug("Found {EventCount} events for aggregate {AggregateId} in version range {FromVersion}-{ToVersion}", 
            filteredEvents.Count, aggregateId, fromVersion, toVersion);
        
        return filteredEvents;
    }

    public async Task<IEnumerable<DomainEvent>> GetEventHistoryAsync(string aggregateId, DateTime fromTimestamp, DateTime toTimestamp, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting event history for aggregate {AggregateId} from {FromTimestamp} to {ToTimestamp}", 
            aggregateId, fromTimestamp, toTimestamp);
        
        var events = await _eventStore.GetEventsAsync(aggregateId, 0, cancellationToken);
        var filteredEvents = events.Where(e => e.Timestamp >= fromTimestamp && e.Timestamp <= toTimestamp).ToList();
        
        _logger.LogDebug("Found {EventCount} events for aggregate {AggregateId} in timestamp range {FromTimestamp}-{ToTimestamp}", 
            filteredEvents.Count, aggregateId, fromTimestamp, toTimestamp);
        
        return filteredEvents;
    }

    public async Task<bool> ValidateEventSequenceAsync(string aggregateId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Validating event sequence for aggregate {AggregateId}", aggregateId);
        
        var events = await _eventStore.GetEventsAsync(aggregateId, 0, cancellationToken);
        var orderedEvents = events.OrderBy(e => e.Version).ToList();
        
        var expectedVersion = 1;
        var isValid = true;
        
        foreach (var domainEvent in orderedEvents)
        {
            if (domainEvent.Version != expectedVersion)
            {
                _logger.LogWarning("Event sequence validation failed for aggregate {AggregateId}: expected version {ExpectedVersion}, found {ActualVersion}", 
                    aggregateId, expectedVersion, domainEvent.Version);
                isValid = false;
                break;
            }
            expectedVersion++;
        }
        
        _logger.LogDebug("Event sequence validation for aggregate {AggregateId}: {IsValid}", aggregateId, isValid);
        
        return isValid;
    }

    public async Task<Dictionary<string, object>> GetAggregateStateAtVersionAsync<T>(string aggregateId, int version, CancellationToken cancellationToken = default) where T : AggregateRoot, new()
    {
        _logger.LogDebug("Getting aggregate state for {AggregateType} with ID {AggregateId} at version {Version}", 
            typeof(T).Name, aggregateId, version);
        
        var aggregate = await ReplayAggregateToVersionAsync<T>(aggregateId, version, cancellationToken);
        
        var state = new Dictionary<string, object>
        {
            { "Id", aggregate.Id },
            { "Version", aggregate.Version },
            { "CreatedAt", aggregate.CreatedAt },
            { "UpdatedAt", aggregate.UpdatedAt },
            { "State", JsonSerializer.Serialize(aggregate, _jsonOptions) }
        };
        
        return state;
    }

    public async Task<Dictionary<string, object>> GetAggregateStateAtTimestampAsync<T>(string aggregateId, DateTime timestamp, CancellationToken cancellationToken = default) where T : AggregateRoot, new()
    {
        _logger.LogDebug("Getting aggregate state for {AggregateType} with ID {AggregateId} at timestamp {Timestamp}", 
            typeof(T).Name, aggregateId, timestamp);
        
        var aggregate = await ReplayAggregateToTimestampAsync<T>(aggregateId, timestamp, cancellationToken);
        
        var state = new Dictionary<string, object>
        {
            { "Id", aggregate.Id },
            { "Version", aggregate.Version },
            { "CreatedAt", aggregate.CreatedAt },
            { "UpdatedAt", aggregate.UpdatedAt },
            { "Timestamp", timestamp },
            { "State", JsonSerializer.Serialize(aggregate, _jsonOptions) }
        };
        
        return state;
    }
}