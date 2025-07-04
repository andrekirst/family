using Family.Api.Data;
using Family.Api.Models.EventStore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Family.Api.Services.EventStore;

public class EventStore : IEventStore
{
    private readonly FamilyDbContext _context;
    private readonly ILogger<EventStore> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public EventStore(FamilyDbContext context, ILogger<EventStore> logger)
    {
        _context = context;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<IEnumerable<DomainEvent>> GetEventsAsync(string aggregateId, int fromVersion = 0, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting events for aggregate {AggregateId} from version {FromVersion}", aggregateId, fromVersion);
        
        var events = await _context.Events
            .Where(e => e.AggregateId == aggregateId && e.Version > fromVersion)
            .OrderBy(e => e.Version)
            .ToListAsync(cancellationToken);

        return events.Select(DeserializeEvent);
    }

    public async Task<IEnumerable<DomainEvent>> GetEventsAsync(string aggregateId, DateTime fromTimestamp, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting events for aggregate {AggregateId} from timestamp {FromTimestamp}", aggregateId, fromTimestamp);
        
        var events = await _context.Events
            .Where(e => e.AggregateId == aggregateId && e.Timestamp >= fromTimestamp)
            .OrderBy(e => e.Timestamp)
            .ToListAsync(cancellationToken);

        return events.Select(DeserializeEvent);
    }

    public async Task<IEnumerable<DomainEvent>> GetEventsByTypeAsync(string eventType, int pageSize = 100, int pageNumber = 1, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting events by type {EventType}, page {PageNumber}, size {PageSize}", eventType, pageNumber, pageSize);
        
        var events = await _context.Events
            .Where(e => e.EventType == eventType)
            .OrderByDescending(e => e.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return events.Select(DeserializeEvent);
    }

    public async Task<IEnumerable<DomainEvent>> GetEventsByAggregateTypeAsync(string aggregateType, int pageSize = 100, int pageNumber = 1, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting events by aggregate type {AggregateType}, page {PageNumber}, size {PageSize}", aggregateType, pageNumber, pageSize);
        
        var events = await _context.Events
            .Where(e => e.AggregateType == aggregateType)
            .OrderByDescending(e => e.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return events.Select(DeserializeEvent);
    }

    public async Task<IEnumerable<DomainEvent>> GetEventsByUserAsync(string userId, int pageSize = 100, int pageNumber = 1, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting events by user {UserId}, page {PageNumber}, size {PageSize}", userId, pageNumber, pageSize);
        
        var events = await _context.Events
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return events.Select(DeserializeEvent);
    }

    public async Task<IEnumerable<DomainEvent>> GetEventsByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting events by correlation ID {CorrelationId}", correlationId);
        
        var events = await _context.Events
            .Where(e => e.CorrelationId == correlationId)
            .OrderBy(e => e.Timestamp)
            .ToListAsync(cancellationToken);

        return events.Select(DeserializeEvent);
    }

    public async Task SaveEventsAsync(string aggregateId, IEnumerable<DomainEvent> events, int expectedVersion, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Saving {EventCount} events for aggregate {AggregateId}, expected version {ExpectedVersion}", 
            events.Count(), aggregateId, expectedVersion);
        
        var currentVersion = await GetLatestVersionAsync(aggregateId, cancellationToken);
        
        if (currentVersion != expectedVersion)
        {
            throw new InvalidOperationException($"Concurrency conflict: expected version {expectedVersion}, but current version is {currentVersion}");
        }

        foreach (var domainEvent in events)
        {
            await SaveEventAsync(domainEvent, cancellationToken);
        }
    }

    public async Task SaveEventAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Saving event {EventId} for aggregate {AggregateId}", domainEvent.EventId, domainEvent.AggregateId);
        
        var eventEntity = new Event
        {
            Id = Guid.Parse(domainEvent.EventId),
            AggregateId = domainEvent.AggregateId,
            AggregateType = domainEvent.AggregateType,
            EventType = domainEvent.EventType,
            EventData = JsonSerializer.Serialize(domainEvent, _jsonOptions),
            Metadata = JsonSerializer.Serialize(domainEvent.Metadata, _jsonOptions),
            Version = domainEvent.Version,
            Timestamp = domainEvent.Timestamp,
            UserId = domainEvent.UserId,
            CorrelationId = domainEvent.CorrelationId,
            CausationId = domainEvent.CausationId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Snapshot?> GetSnapshotAsync(string aggregateId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting snapshot for aggregate {AggregateId}", aggregateId);
        
        return await _context.Snapshots
            .Where(s => s.AggregateId == aggregateId)
            .OrderByDescending(s => s.Version)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task SaveSnapshotAsync(Snapshot snapshot, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Saving snapshot for aggregate {AggregateId} at version {Version}", 
            snapshot.AggregateId, snapshot.Version);
        
        _context.Snapshots.Add(snapshot);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> EventExistsAsync(string eventId, CancellationToken cancellationToken = default)
    {
        var eventGuid = Guid.Parse(eventId);
        return await _context.Events.AnyAsync(e => e.Id == eventGuid, cancellationToken);
    }

    public async Task<int> GetLatestVersionAsync(string aggregateId, CancellationToken cancellationToken = default)
    {
        var latestEvent = await _context.Events
            .Where(e => e.AggregateId == aggregateId)
            .OrderByDescending(e => e.Version)
            .FirstOrDefaultAsync(cancellationToken);

        return latestEvent?.Version ?? 0;
    }

    private DomainEvent DeserializeEvent(Event eventEntity)
    {
        var eventType = Type.GetType($"Family.Api.Models.EventStore.{eventEntity.EventType}");
        if (eventType == null)
        {
            throw new InvalidOperationException($"Unknown event type: {eventEntity.EventType}");
        }

        var domainEvent = JsonSerializer.Deserialize(eventEntity.EventData, eventType, _jsonOptions) as DomainEvent;
        if (domainEvent == null)
        {
            throw new InvalidOperationException($"Failed to deserialize event {eventEntity.Id}");
        }

        return domainEvent;
    }
}