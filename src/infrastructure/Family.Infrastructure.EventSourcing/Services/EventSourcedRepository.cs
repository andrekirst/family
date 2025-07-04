using Family.Infrastructure.EventSourcing.Models;
using Family.Infrastructure.EventSourcing.Models;

namespace Family.Infrastructure.EventSourcing.Services;

public class EventSourcedRepository<T> : IEventSourcedRepository<T> where T : AggregateRoot, new()
{
    private readonly IEventStore _eventStore;
    private readonly ILogger<EventSourcedRepository<T>> _logger;
    private readonly int _snapshotFrequency;

    public EventSourcedRepository(IEventStore eventStore, ILogger<EventSourcedRepository<T>> logger, int snapshotFrequency = 100)
    {
        _eventStore = eventStore;
        _logger = logger;
        _snapshotFrequency = snapshotFrequency;
    }

    public async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting aggregate {AggregateType} with ID {Id}", typeof(T).Name, id);
        
        var snapshot = await _eventStore.GetSnapshotAsync(id, cancellationToken);
        var fromVersion = snapshot?.Version ?? 0;
        
        var events = await _eventStore.GetEventsAsync(id, fromVersion, cancellationToken);
        
        if (!events.Any() && snapshot == null)
        {
            return null;
        }
        
        var aggregate = new T();
        
        if (snapshot != null)
        {
            aggregate = RestoreFromSnapshot(snapshot);
        }
        
        aggregate.LoadFromHistory(events);
        
        return aggregate;
    }

    public async Task<T?> GetByIdAsync(string id, int version, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting aggregate {AggregateType} with ID {Id} at version {Version}", typeof(T).Name, id, version);
        
        var events = await _eventStore.GetEventsAsync(id, 0, cancellationToken);
        var eventsUpToVersion = events.Where(e => e.Version <= version).ToList();
        
        if (!eventsUpToVersion.Any())
        {
            return null;
        }
        
        var aggregate = new T();
        aggregate.LoadFromHistory(eventsUpToVersion);
        
        return aggregate;
    }

    public async Task<T> GetByIdAsync(string id, DateTime timestamp, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting aggregate {AggregateType} with ID {Id} at timestamp {Timestamp}", typeof(T).Name, id, timestamp);
        
        var events = await _eventStore.GetEventsAsync(id, timestamp, cancellationToken);
        var eventsUpToTimestamp = events.Where(e => e.Timestamp <= timestamp).ToList();
        
        if (!eventsUpToTimestamp.Any())
        {
            throw new InvalidOperationException($"No events found for aggregate {id} up to timestamp {timestamp}");
        }
        
        var aggregate = new T();
        aggregate.LoadFromHistory(eventsUpToTimestamp);
        
        return aggregate;
    }

    public async Task SaveAsync(T aggregate, CancellationToken cancellationToken = default)
    {
        await SaveAsync(aggregate, aggregate.Version, cancellationToken);
    }

    public async Task SaveAsync(T aggregate, int expectedVersion, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Saving aggregate {AggregateType} with ID {Id}, expected version {ExpectedVersion}", 
            typeof(T).Name, aggregate.Id, expectedVersion);
        
        var uncommittedEvents = aggregate.UncommittedEvents.ToList();
        
        if (!uncommittedEvents.Any())
        {
            _logger.LogDebug("No uncommitted events for aggregate {Id}", aggregate.Id);
            return;
        }
        
        await _eventStore.SaveEventsAsync(aggregate.Id, uncommittedEvents, expectedVersion, cancellationToken);
        
        aggregate.MarkEventsAsCommitted();
        
        // Create snapshot if needed
        if (aggregate.Version % _snapshotFrequency == 0)
        {
            await CreateSnapshotAsync(aggregate, cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        var latestVersion = await _eventStore.GetLatestVersionAsync(id, cancellationToken);
        return latestVersion > 0;
    }

    public async Task<IEnumerable<T>> GetAllAsync(int pageSize = 100, int pageNumber = 1, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all aggregates of type {AggregateType}, page {PageNumber}, size {PageSize}", 
            typeof(T).Name, pageNumber, pageSize);
        
        var events = await _eventStore.GetEventsByAggregateTypeAsync(typeof(T).Name, pageSize * 10, pageNumber, cancellationToken);
        
        var aggregateIds = events.Select(e => e.AggregateId).Distinct().ToList();
        var aggregates = new List<T>();
        
        foreach (var aggregateId in aggregateIds)
        {
            var aggregate = await GetByIdAsync(aggregateId, cancellationToken);
            if (aggregate != null)
            {
                aggregates.Add(aggregate);
            }
        }
        
        return aggregates;
    }

    public async Task<T> ReplayToVersionAsync(string id, int version, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Replaying aggregate {AggregateType} with ID {Id} to version {Version}", typeof(T).Name, id, version);
        
        var events = await _eventStore.GetEventsAsync(id, 0, cancellationToken);
        var eventsUpToVersion = events.Where(e => e.Version <= version).ToList();
        
        if (!eventsUpToVersion.Any())
        {
            throw new InvalidOperationException($"No events found for aggregate {id} up to version {version}");
        }
        
        var aggregate = new T();
        aggregate.ReplayEvents(eventsUpToVersion);
        
        return aggregate;
    }

    public async Task<T> ReplayToTimestampAsync(string id, DateTime timestamp, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Replaying aggregate {AggregateType} with ID {Id} to timestamp {Timestamp}", typeof(T).Name, id, timestamp);
        
        var events = await _eventStore.GetEventsAsync(id, 0, cancellationToken);
        var eventsUpToTimestamp = events.Where(e => e.Timestamp <= timestamp).ToList();
        
        if (!eventsUpToTimestamp.Any())
        {
            throw new InvalidOperationException($"No events found for aggregate {id} up to timestamp {timestamp}");
        }
        
        var aggregate = new T();
        aggregate.ReplayEvents(eventsUpToTimestamp);
        
        return aggregate;
    }

    private async Task CreateSnapshotAsync(T aggregate, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Creating snapshot for aggregate {AggregateType} with ID {Id} at version {Version}", 
            typeof(T).Name, aggregate.Id, aggregate.Version);
        
        var snapshot = new Snapshot
        {
            Id = Guid.NewGuid(),
            AggregateId = aggregate.Id,
            AggregateType = typeof(T).Name,
            Data = System.Text.Json.JsonSerializer.Serialize(aggregate),
            Version = aggregate.Version,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        await _eventStore.SaveSnapshotAsync(snapshot, cancellationToken);
    }

    private T RestoreFromSnapshot(Snapshot snapshot)
    {
        _logger.LogDebug("Restoring aggregate {AggregateType} from snapshot at version {Version}", 
            typeof(T).Name, snapshot.Version);
        
        var aggregate = System.Text.Json.JsonSerializer.Deserialize<T>(snapshot.Data);
        
        if (aggregate == null)
        {
            throw new InvalidOperationException($"Failed to restore aggregate from snapshot {snapshot.Id}");
        }
        
        return aggregate;
    }
}