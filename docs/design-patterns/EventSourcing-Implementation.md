# Event Sourcing - Implementation Guide

## Übersicht

Event Sourcing speichert alle Änderungen als unveränderliche Events in einem append-only Store. Statt den aktuellen Zustand zu speichern, werden alle Ereignisse gespeichert und der Zustand durch Event-Replay rekonstruiert.

## Architektur-Überblick

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Commands      │    │   Domain        │    │   Event Store   │
│   (GraphQL)     │───▶│   Aggregates    │───▶│   (PostgreSQL)  │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                │                        │
                                ▼                        │
                       ┌─────────────────┐               │
                       │   Domain        │               │
                       │   Events        │               │
                       └─────────┬───────┘               │
                                 │                       │
                                 ▼                       │
                       ┌─────────────────┐               │
                       │   Event         │               │
                       │   Handlers      │               │
                       └─────────┬───────┘               │
                                 │                       │
                                 ▼                       │
                       ┌─────────────────┐               │
                       │   Read Models   │◀──────────────┘
                       │   (Projections) │
                       └─────────────────┘
```

## 1. Domain Events Definition

### Base Event Classes
```csharp
// Domain/Events/DomainEvent.cs
public abstract record DomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string EventType { get; init; } = string.Empty;
    public int Version { get; init; } = 1;
    public Guid AggregateId { get; init; }
    public string AggregateName { get; init; } = string.Empty;

    protected DomainEvent()
    {
        EventType = GetType().Name;
    }
}

// Domain/Events/IEventStore.cs
public interface IEventStore
{
    Task SaveEventsAsync<T>(
        Guid aggregateId, 
        IEnumerable<DomainEvent> events, 
        int expectedVersion,
        CancellationToken cancellationToken = default) where T : AggregateRoot;

    Task<IEnumerable<DomainEvent>> GetEventsAsync(
        Guid aggregateId,
        int fromVersion = 0,
        CancellationToken cancellationToken = default);

    Task<T?> GetAggregateAsync<T>(
        Guid aggregateId,
        CancellationToken cancellationToken = default) where T : AggregateRoot, new();
}
```

### Family Domain Events
```csharp
// Domain/Events/FamilyEvents.cs
public record FamilyCreatedEvent(
    Guid FamilyId,
    string Name,
    Guid CreatedBy,
    DateTime CreatedAt
) : DomainEvent
{
    public override Guid AggregateId { get; init; } = FamilyId;
    public override string AggregateName { get; init; } = nameof(Family);
}

public record FamilyMemberAddedEvent(
    Guid FamilyId,
    Guid MemberId,
    string FirstName,
    string LastName,
    string Email,
    DateOnly DateOfBirth,
    Guid AddedBy,
    DateTime AddedAt
) : DomainEvent
{
    public override Guid AggregateId { get; init; } = FamilyId;
    public override string AggregateName { get; init; } = nameof(Family);
    
    public string FullName => $"{FirstName} {LastName}";
}

public record FamilyMemberUpdatedEvent(
    Guid FamilyId,
    Guid MemberId,
    string? FirstName,
    string? LastName,
    string? Email,
    DateOnly? DateOfBirth,
    Guid UpdatedBy,
    DateTime UpdatedAt
) : DomainEvent
{
    public override Guid AggregateId { get; init; } = FamilyId;
    public override string AggregateName { get; init; } = nameof(Family);
}

public record FamilyMemberRemovedEvent(
    Guid FamilyId,
    Guid MemberId,
    string Reason,
    Guid RemovedBy,
    DateTime RemovedAt
) : DomainEvent
{
    public override Guid AggregateId { get; init; } = FamilyId;
    public override string AggregateName { get; init; } = nameof(Family);
}

// Medical Domain Events
public record MedicationScheduledEvent(
    Guid FamilyId,
    Guid MemberId,
    Guid MedicationId,
    string MedicationName,
    string Dosage,
    TimeOnly ScheduledTime,
    DateTime ScheduledDate,
    Guid ScheduledBy
) : DomainEvent
{
    public override Guid AggregateId { get; init; } = MemberId;
    public override string AggregateName { get; init; } = nameof(FamilyMember);
}

public record MedicationTakenEvent(
    Guid FamilyId,
    Guid MemberId,
    Guid MedicationId,
    DateTime TakenAt,
    string? Notes,
    Guid RecordedBy
) : DomainEvent
{
    public override Guid AggregateId { get; init; } = MemberId;
    public override string AggregateName { get; init; } = nameof(FamilyMember);
}
```

## 2. Aggregate Root mit Event Sourcing

### Base Aggregate Root
```csharp
// Domain/AggregateRoot.cs
public abstract class AggregateRoot
{
    private readonly List<DomainEvent> _uncommittedEvents = [];
    
    public Guid Id { get; protected set; }
    public int Version { get; protected set; } = 0;

    public IReadOnlyList<DomainEvent> GetUncommittedEvents() 
        => _uncommittedEvents.AsReadOnly();

    public void MarkEventsAsCommitted() 
        => _uncommittedEvents.Clear();

    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        _uncommittedEvents.Add(domainEvent);
        Version++;
    }

    public void LoadFromHistory(IEnumerable<DomainEvent> events)
    {
        foreach (var @event in events.OrderBy(e => e.Version))
        {
            ApplyEvent(@event, false);
        }
    }

    protected void ApplyEvent(DomainEvent @event, bool isNew = true)
    {
        // Use reflection to find and call the appropriate Apply method
        var applyMethod = GetType().GetMethod("Apply", [@event.GetType()]);
        applyMethod?.Invoke(this, [@event]);

        if (isNew)
        {
            AddDomainEvent(@event);
        }
        else
        {
            Version++;
        }
    }
}
```

### Family Aggregate with Event Sourcing
```csharp
// Domain/Family.cs
public class Family : AggregateRoot
{
    private readonly Dictionary<Guid, FamilyMember> _members = [];
    
    public string Name { get; private set; } = string.Empty;
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyCollection<FamilyMember> Members => _members.Values;

    // Required for Event Sourcing reconstruction
    public Family() { }

    // Factory method for new aggregates
    public static Family Create(string name, Guid createdBy)
    {
        var family = new Family();
        var @event = new FamilyCreatedEvent(
            Guid.NewGuid(),
            name,
            createdBy,
            DateTime.UtcNow);
        
        family.ApplyEvent(@event);
        return family;
    }

    // Commands that generate events
    public FamilyMember AddMember(
        string firstName, 
        string lastName, 
        string email, 
        DateOnly dateOfBirth, 
        Guid addedBy)
    {
        // Business logic validation
        if (_members.Values.Any(m => m.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException("Ein Familienmitglied mit dieser E-Mail existiert bereits");

        var memberId = Guid.NewGuid();
        var @event = new FamilyMemberAddedEvent(
            Id,
            memberId,
            firstName,
            lastName,
            email,
            dateOfBirth,
            addedBy,
            DateTime.UtcNow);

        ApplyEvent(@event);
        return _members[memberId];
    }

    public void UpdateMember(
        Guid memberId, 
        string? firstName = null, 
        string? lastName = null, 
        string? email = null,
        DateOnly? dateOfBirth = null,
        Guid updatedBy = default)
    {
        if (!_members.ContainsKey(memberId))
            throw new DomainException("Familienmitglied nicht gefunden");

        // Validate email uniqueness if email is being updated
        if (email != null && _members.Values.Any(m => 
            m.Id != memberId && m.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException("Ein anderes Familienmitglied mit dieser E-Mail existiert bereits");

        var @event = new FamilyMemberUpdatedEvent(
            Id,
            memberId,
            firstName,
            lastName,
            email,
            dateOfBirth,
            updatedBy,
            DateTime.UtcNow);

        ApplyEvent(@event);
    }

    public void RemoveMember(Guid memberId, string reason, Guid removedBy)
    {
        if (!_members.ContainsKey(memberId))
            throw new DomainException("Familienmitglied nicht gefunden");

        var @event = new FamilyMemberRemovedEvent(
            Id,
            memberId,
            reason,
            removedBy,
            DateTime.UtcNow);

        ApplyEvent(@event);
    }

    // Event Application Methods (for state reconstruction)
    public void Apply(FamilyCreatedEvent @event)
    {
        Id = @event.FamilyId;
        Name = @event.Name;
        CreatedBy = @event.CreatedBy;
        CreatedAt = @event.CreatedAt;
    }

    public void Apply(FamilyMemberAddedEvent @event)
    {
        var member = new FamilyMember
        {
            Id = @event.MemberId,
            FirstName = @event.FirstName,
            LastName = @event.LastName,
            Email = @event.Email,
            DateOfBirth = @event.DateOfBirth,
            FamilyId = Id
        };
        
        _members[@event.MemberId] = member;
    }

    public void Apply(FamilyMemberUpdatedEvent @event)
    {
        if (_members.TryGetValue(@event.MemberId, out var member))
        {
            if (@event.FirstName != null) member.FirstName = @event.FirstName;
            if (@event.LastName != null) member.LastName = @event.LastName;
            if (@event.Email != null) member.Email = @event.Email;
            if (@event.DateOfBirth.HasValue) member.DateOfBirth = @event.DateOfBirth.Value;
        }
    }

    public void Apply(FamilyMemberRemovedEvent @event)
    {
        _members.Remove(@event.MemberId);
    }
}

// Domain/FamilyMember.cs (simplified for Event Sourcing)
public class FamilyMember
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public Guid FamilyId { get; set; }
    
    public string FullName => $"{FirstName} {LastName}";
    public int Age => CalculateAge(DateOfBirth);

    private static int CalculateAge(DateOnly birthDate)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - birthDate.Year;
        if (birthDate > today.AddYears(-age)) age--;
        return age;
    }
}
```

## 3. Event Store Implementation

### PostgreSQL Event Store
```csharp
// Infrastructure/EventStore/PostgreSqlEventStore.cs
public class PostgreSqlEventStore : IEventStore
{
    private readonly EventStoreDbContext _context;
    private readonly ILogger<PostgreSqlEventStore> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public PostgreSqlEventStore(
        EventStoreDbContext context, 
        ILogger<PostgreSqlEventStore> logger)
    {
        _context = context;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task SaveEventsAsync<T>(
        Guid aggregateId, 
        IEnumerable<DomainEvent> events, 
        int expectedVersion,
        CancellationToken cancellationToken = default) where T : AggregateRoot
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // Check for concurrency conflicts
            var currentVersion = await GetCurrentVersionAsync(aggregateId, cancellationToken);
            if (currentVersion != expectedVersion)
            {
                throw new ConcurrencyException(
                    $"Concurrency conflict for aggregate {aggregateId}. " +
                    $"Expected version {expectedVersion}, but current version is {currentVersion}");
            }

            var eventRecords = events.Select((domainEvent, index) => new EventRecord
            {
                Id = Guid.NewGuid(),
                AggregateId = aggregateId,
                AggregateName = typeof(T).Name,
                EventType = domainEvent.GetType().Name,
                EventData = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), _jsonOptions),
                Version = expectedVersion + index + 1,
                OccurredAt = domainEvent.OccurredAt,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            _context.Events.AddRange(eventRecords);
            await _context.SaveChangesAsync(cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Saved {EventCount} events for aggregate {AggregateId} (type: {AggregateType})",
                eventRecords.Count, aggregateId, typeof(T).Name);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, 
                "Failed to save events for aggregate {AggregateId}", aggregateId);
            throw;
        }
    }

    public async Task<IEnumerable<DomainEvent>> GetEventsAsync(
        Guid aggregateId,
        int fromVersion = 0,
        CancellationToken cancellationToken = default)
    {
        var eventRecords = await _context.Events
            .Where(e => e.AggregateId == aggregateId && e.Version > fromVersion)
            .OrderBy(e => e.Version)
            .ToListAsync(cancellationToken);

        var events = new List<DomainEvent>();
        
        foreach (var record in eventRecords)
        {
            var eventType = GetEventType(record.EventType);
            if (eventType != null)
            {
                var domainEvent = JsonSerializer.Deserialize(record.EventData, eventType, _jsonOptions) as DomainEvent;
                if (domainEvent != null)
                {
                    events.Add(domainEvent);
                }
            }
        }

        return events;
    }

    public async Task<T?> GetAggregateAsync<T>(
        Guid aggregateId,
        CancellationToken cancellationToken = default) where T : AggregateRoot, new()
    {
        var events = await GetEventsAsync(aggregateId, 0, cancellationToken);
        
        if (!events.Any())
            return null;

        var aggregate = new T();
        aggregate.LoadFromHistory(events);
        
        return aggregate;
    }

    private async Task<int> GetCurrentVersionAsync(Guid aggregateId, CancellationToken cancellationToken)
    {
        return await _context.Events
            .Where(e => e.AggregateId == aggregateId)
            .MaxAsync(e => (int?)e.Version, cancellationToken) ?? 0;
    }

    private static Type? GetEventType(string eventTypeName)
    {
        // Cache this lookup in production
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .FirstOrDefault(t => t.Name == eventTypeName && typeof(DomainEvent).IsAssignableFrom(t));
    }
}

// Infrastructure/EventStore/EventRecord.cs
public class EventRecord
{
    public Guid Id { get; set; }
    public Guid AggregateId { get; set; }
    public string AggregateName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public int Version { get; set; }
    public DateTime OccurredAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Infrastructure/EventStore/EventStoreDbContext.cs
public class EventStoreDbContext : DbContext
{
    public DbSet<EventRecord> Events { get; set; }

    public EventStoreDbContext(DbContextOptions<EventStoreDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.AggregateId).IsRequired();
            entity.Property(e => e.AggregateName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EventData).IsRequired().HasColumnType("jsonb");
            entity.Property(e => e.Version).IsRequired();
            entity.Property(e => e.OccurredAt).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            // Unique constraint for aggregate + version
            entity.HasIndex(e => new { e.AggregateId, e.Version })
                  .IsUnique()
                  .HasDatabaseName("IX_Events_AggregateId_Version");

            // Index for querying events by aggregate
            entity.HasIndex(e => e.AggregateId)
                  .HasDatabaseName("IX_Events_AggregateId");

            // Index for event type queries
            entity.HasIndex(e => e.EventType)
                  .HasDatabaseName("IX_Events_EventType");
        });
    }
}
```

## 4. Repository Implementation

### Event-Sourced Repository
```csharp
// Infrastructure/Repositories/EventSourcedFamilyRepository.cs
public class EventSourcedFamilyRepository : IFamilyRepository
{
    private readonly IEventStore _eventStore;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<EventSourcedFamilyRepository> _logger;

    public EventSourcedFamilyRepository(
        IEventStore eventStore,
        IEventPublisher eventPublisher,
        ILogger<EventSourcedFamilyRepository> logger)
    {
        _eventStore = eventStore;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<Family?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Loading family aggregate {FamilyId} from event store", id);
        
        var family = await _eventStore.GetAggregateAsync<Family>(id, cancellationToken);
        
        if (family != null)
        {
            _logger.LogDebug(
                "Loaded family aggregate {FamilyId} with version {Version}", 
                id, family.Version);
        }
        
        return family;
    }

    public async Task SaveAsync(Family family, CancellationToken cancellationToken = default)
    {
        var uncommittedEvents = family.GetUncommittedEvents();
        
        if (!uncommittedEvents.Any())
        {
            _logger.LogDebug("No uncommitted events for family {FamilyId}", family.Id);
            return;
        }

        _logger.LogDebug(
            "Saving {EventCount} events for family {FamilyId}", 
            uncommittedEvents.Count, family.Id);

        // Calculate expected version (current version minus uncommitted events)
        var expectedVersion = family.Version - uncommittedEvents.Count;

        await _eventStore.SaveEventsAsync<Family>(
            family.Id, 
            uncommittedEvents, 
            expectedVersion,
            cancellationToken);

        // Publish events to external systems
        foreach (var domainEvent in uncommittedEvents)
        {
            await _eventPublisher.PublishAsync(domainEvent, cancellationToken);
        }

        family.MarkEventsAsCommitted();

        _logger.LogInformation(
            "Successfully saved and published {EventCount} events for family {FamilyId}",
            uncommittedEvents.Count, family.Id);
    }
}
```

## 5. Event Handlers for Read Model Projections

### Family Read Model Projection Handler
```csharp
// Application/EventHandlers/FamilyProjectionHandler.cs
public class FamilyProjectionHandler :
    IEventHandler<FamilyCreatedEvent>,
    IEventHandler<FamilyMemberAddedEvent>,
    IEventHandler<FamilyMemberUpdatedEvent>,
    IEventHandler<FamilyMemberRemovedEvent>
{
    private readonly ReadDbContext _readContext;
    private readonly ILogger<FamilyProjectionHandler> _logger;

    public FamilyProjectionHandler(
        ReadDbContext readContext,
        ILogger<FamilyProjectionHandler> logger)
    {
        _readContext = readContext;
        _logger = logger;
    }

    public async Task Handle(FamilyCreatedEvent @event, CancellationToken cancellationToken)
    {
        var familyReadModel = new FamilyReadModel
        {
            Id = @event.FamilyId,
            Name = @event.Name,
            CreatedBy = @event.CreatedBy,
            CreatedAt = @event.CreatedAt,
            MemberCount = 0,
            LastUpdated = @event.CreatedAt
        };

        _readContext.Families.Add(familyReadModel);
        await _readContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created family read model for {FamilyId}", @event.FamilyId);
    }

    public async Task Handle(FamilyMemberAddedEvent @event, CancellationToken cancellationToken)
    {
        // Update family read model
        var family = await _readContext.Families
            .FirstOrDefaultAsync(f => f.Id == @event.FamilyId, cancellationToken);
        
        if (family != null)
        {
            family.MemberCount++;
            family.LastUpdated = @event.AddedAt;
        }

        // Create family member read model
        var memberReadModel = new FamilyMemberReadModel
        {
            Id = @event.MemberId,
            FirstName = @event.FirstName,
            LastName = @event.LastName,
            Email = @event.Email,
            DateOfBirth = @event.DateOfBirth,
            FamilyId = @event.FamilyId,
            CreatedAt = @event.AddedAt,
            LastUpdated = @event.AddedAt
        };

        _readContext.FamilyMembers.Add(memberReadModel);
        await _readContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Added family member {MemberId} to read model for family {FamilyId}",
            @event.MemberId, @event.FamilyId);
    }

    public async Task Handle(FamilyMemberUpdatedEvent @event, CancellationToken cancellationToken)
    {
        var member = await _readContext.FamilyMembers
            .FirstOrDefaultAsync(m => m.Id == @event.MemberId, cancellationToken);

        if (member != null)
        {
            if (@event.FirstName != null) member.FirstName = @event.FirstName;
            if (@event.LastName != null) member.LastName = @event.LastName;
            if (@event.Email != null) member.Email = @event.Email;
            if (@event.DateOfBirth.HasValue) member.DateOfBirth = @event.DateOfBirth.Value;
            
            member.LastUpdated = @event.UpdatedAt;

            await _readContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Updated family member {MemberId} in read model", @event.MemberId);
        }
    }

    public async Task Handle(FamilyMemberRemovedEvent @event, CancellationToken cancellationToken)
    {
        // Update family read model
        var family = await _readContext.Families
            .FirstOrDefaultAsync(f => f.Id == @event.FamilyId, cancellationToken);
        
        if (family != null)
        {
            family.MemberCount--;
            family.LastUpdated = @event.RemovedAt;
        }

        // Remove family member read model
        var member = await _readContext.FamilyMembers
            .FirstOrDefaultAsync(m => m.Id == @event.MemberId, cancellationToken);
        
        if (member != null)
        {
            _readContext.FamilyMembers.Remove(member);
        }

        await _readContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Removed family member {MemberId} from read model for family {FamilyId}",
            @event.MemberId, @event.FamilyId);
    }
}

// Read Models
public class FamilyReadModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MemberCount { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class FamilyMemberReadModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public Guid FamilyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }
    
    public string FullName => $"{FirstName} {LastName}";
}
```

## 6. Event Replay and Snapshots

### Event Replay Service
```csharp
// Application/Services/EventReplayService.cs
public class EventReplayService : IEventReplayService
{
    private readonly IEventStore _eventStore;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventReplayService> _logger;

    public EventReplayService(
        IEventStore eventStore,
        IServiceProvider serviceProvider,
        ILogger<EventReplayService> logger)
    {
        _eventStore = eventStore;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ReplayAllEventsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting full event replay...");

        // Get all events ordered by creation time
        var allEvents = await GetAllEventsAsync(cancellationToken);
        
        using var scope = _serviceProvider.CreateScope();
        var readContext = scope.ServiceProvider.GetRequiredService<ReadDbContext>();
        
        // Clear read models
        await ClearReadModelsAsync(readContext, cancellationToken);
        
        // Replay events
        var eventCount = 0;
        foreach (var @event in allEvents)
        {
            await ProcessEventAsync(@event, scope.ServiceProvider, cancellationToken);
            eventCount++;
            
            if (eventCount % 1000 == 0)
            {
                _logger.LogInformation("Processed {EventCount} events", eventCount);
            }
        }

        _logger.LogInformation("Event replay completed. Processed {TotalEvents} events", eventCount);
    }

    public async Task ReplayEventsForAggregateAsync<T>(
        Guid aggregateId, 
        CancellationToken cancellationToken = default) where T : AggregateRoot
    {
        _logger.LogInformation("Replaying events for aggregate {AggregateId}", aggregateId);

        var events = await _eventStore.GetEventsAsync(aggregateId, 0, cancellationToken);
        
        using var scope = _serviceProvider.CreateScope();
        
        foreach (var @event in events)
        {
            await ProcessEventAsync(@event, scope.ServiceProvider, cancellationToken);
        }

        _logger.LogInformation(
            "Completed event replay for aggregate {AggregateId}. Processed {EventCount} events",
            aggregateId, events.Count());
    }

    private async Task<IEnumerable<DomainEvent>> GetAllEventsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var eventStoreContext = scope.ServiceProvider.GetRequiredService<EventStoreDbContext>();
        
        var eventRecords = await eventStoreContext.Events
            .OrderBy(e => e.CreatedAt)
            .ThenBy(e => e.Version)
            .ToListAsync(cancellationToken);

        var events = new List<DomainEvent>();
        foreach (var record in eventRecords)
        {
            var eventType = GetEventType(record.EventType);
            if (eventType != null)
            {
                var domainEvent = JsonSerializer.Deserialize(
                    record.EventData, eventType) as DomainEvent;
                if (domainEvent != null)
                {
                    events.Add(domainEvent);
                }
            }
        }

        return events;
    }

    private async Task ProcessEventAsync(
        DomainEvent @event, 
        IServiceProvider serviceProvider, 
        CancellationToken cancellationToken)
    {
        var eventType = @event.GetType();
        var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
        var handlers = serviceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            var handleMethod = handlerType.GetMethod("Handle");
            if (handleMethod != null)
            {
                await (Task)handleMethod.Invoke(handler, [@event, cancellationToken])!;
            }
        }
    }

    private async Task ClearReadModelsAsync(ReadDbContext readContext, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Clearing read models for replay...");
        
        readContext.FamilyMembers.RemoveRange(readContext.FamilyMembers);
        readContext.Families.RemoveRange(readContext.Families);
        
        await readContext.SaveChangesAsync(cancellationToken);
    }

    private static Type? GetEventType(string eventTypeName)
    {
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .FirstOrDefault(t => t.Name == eventTypeName && typeof(DomainEvent).IsAssignableFrom(t));
    }
}
```

## 7. Testing Event Sourcing

### Aggregate Tests with Event Sourcing
```csharp
// Tests/Unit/FamilyAggregateTests.cs
public class FamilyAggregateTests
{
    [Fact]
    public void Create_NewFamily_GeneratesFamilyCreatedEvent()
    {
        // Arrange
        var name = "Test Family";
        var createdBy = Guid.NewGuid();

        // Act
        var family = Family.Create(name, createdBy);

        // Assert
        family.Name.Should().Be(name);
        family.CreatedBy.Should().Be(createdBy);
        
        var uncommittedEvents = family.GetUncommittedEvents();
        uncommittedEvents.Should().HaveCount(1);
        
        var familyCreatedEvent = uncommittedEvents.First().Should().BeOfType<FamilyCreatedEvent>().Subject;
        familyCreatedEvent.Name.Should().Be(name);
        familyCreatedEvent.CreatedBy.Should().Be(createdBy);
    }

    [Fact]
    public void AddMember_ValidMember_GeneratesFamilyMemberAddedEvent()
    {
        // Arrange
        var family = Family.Create("Test Family", Guid.NewGuid());
        family.MarkEventsAsCommitted(); // Clear creation event
        
        var addedBy = Guid.NewGuid();

        // Act
        var member = family.AddMember("John", "Doe", "john.doe@example.com", 
            new DateOnly(1990, 1, 1), addedBy);

        // Assert
        member.FullName.Should().Be("John Doe");
        family.Members.Should().HaveCount(1);
        
        var uncommittedEvents = family.GetUncommittedEvents();
        uncommittedEvents.Should().HaveCount(1);
        
        var memberAddedEvent = uncommittedEvents.First().Should().BeOfType<FamilyMemberAddedEvent>().Subject;
        memberAddedEvent.FirstName.Should().Be("John");
        memberAddedEvent.LastName.Should().Be("Doe");
        memberAddedEvent.Email.Should().Be("john.doe@example.com");
    }

    [Fact]
    public void LoadFromHistory_EventSequence_ReconstructsCorrectState()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var addedBy = Guid.NewGuid();
        
        var events = new List<DomainEvent>
        {
            new FamilyCreatedEvent(familyId, "Test Family", createdBy, DateTime.UtcNow),
            new FamilyMemberAddedEvent(familyId, Guid.NewGuid(), "John", "Doe", 
                "john.doe@example.com", new DateOnly(1990, 1, 1), addedBy, DateTime.UtcNow),
            new FamilyMemberAddedEvent(familyId, Guid.NewGuid(), "Jane", "Doe", 
                "jane.doe@example.com", new DateOnly(1992, 5, 15), addedBy, DateTime.UtcNow)
        };

        // Act
        var family = new Family();
        family.LoadFromHistory(events);

        // Assert
        family.Id.Should().Be(familyId);
        family.Name.Should().Be("Test Family");
        family.Members.Should().HaveCount(2);
        family.Members.Should().Contain(m => m.FirstName == "John");
        family.Members.Should().Contain(m => m.FirstName == "Jane");
        family.Version.Should().Be(3);
        family.GetUncommittedEvents().Should().BeEmpty();
    }
}
```

### Event Store Tests
```csharp
// Tests/Integration/PostgreSqlEventStoreTests.cs
public class PostgreSqlEventStoreTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly PostgreSqlEventStore _eventStore;

    public PostgreSqlEventStoreTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        _eventStore = new PostgreSqlEventStore(_fixture.EventStoreContext, 
            Substitute.For<ILogger<PostgreSqlEventStore>>());
    }

    [Fact]
    public async Task SaveEventsAsync_NewEvents_SavesSuccessfully()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var events = new List<DomainEvent>
        {
            new FamilyCreatedEvent(aggregateId, "Test Family", Guid.NewGuid(), DateTime.UtcNow),
            new FamilyMemberAddedEvent(aggregateId, Guid.NewGuid(), "John", "Doe", 
                "john.doe@example.com", new DateOnly(1990, 1, 1), Guid.NewGuid(), DateTime.UtcNow)
        };

        // Act
        await _eventStore.SaveEventsAsync<Family>(aggregateId, events, 0);

        // Assert
        var savedEvents = await _eventStore.GetEventsAsync(aggregateId);
        savedEvents.Should().HaveCount(2);
        savedEvents.First().Should().BeOfType<FamilyCreatedEvent>();
        savedEvents.Last().Should().BeOfType<FamilyMemberAddedEvent>();
    }

    [Fact]
    public async Task GetAggregateAsync_ExistingAggregate_ReconstructsCorrectly()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var events = new List<DomainEvent>
        {
            new FamilyCreatedEvent(aggregateId, "Test Family", createdBy, DateTime.UtcNow)
        };
        
        await _eventStore.SaveEventsAsync<Family>(aggregateId, events, 0);

        // Act
        var family = await _eventStore.GetAggregateAsync<Family>(aggregateId);

        // Assert
        family.Should().NotBeNull();
        family!.Id.Should().Be(aggregateId);
        family.Name.Should().Be("Test Family");
        family.CreatedBy.Should().Be(createdBy);
        family.Version.Should().Be(1);
    }

    [Fact]
    public async Task SaveEventsAsync_ConcurrencyConflict_ThrowsConcurrencyException()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var initialEvent = new FamilyCreatedEvent(aggregateId, "Test Family", Guid.NewGuid(), DateTime.UtcNow);
        
        await _eventStore.SaveEventsAsync<Family>(aggregateId, [initialEvent], 0);

        var conflictingEvent = new FamilyMemberAddedEvent(aggregateId, Guid.NewGuid(), 
            "John", "Doe", "john.doe@example.com", new DateOnly(1990, 1, 1), 
            Guid.NewGuid(), DateTime.UtcNow);

        // Act & Assert
        await FluentActions.Invoking(() => 
                _eventStore.SaveEventsAsync<Family>(aggregateId, [conflictingEvent], 0))
            .Should().ThrowAsync<ConcurrencyException>()
            .WithMessage("*Concurrency conflict*");
    }
}
```

## 8. Benefits and Considerations

### Vorteile
1. **Vollständige Auditierbarkeit**: Alle Änderungen sind nachvollziehbar
2. **Zeitreise**: Zustand zu jedem Zeitpunkt rekonstruierbar
3. **Debugging**: Event-Replay für Problemanalyse
4. **Skalierbarkeit**: Read Models können unabhängig optimiert werden
5. **Integration**: Events für andere Services verfügbar

### Herausforderungen
1. **Komplexität**: Höhere Komplexität als CRUD-Ansätze
2. **Eventual Consistency**: Read Models sind eventuell nicht sofort aktuell
3. **Event Schema Evolution**: Versioning von Events notwendig
4. **Storage**: Mehr Speicherverbrauch durch Event-History
5. **Performance**: Event-Replay kann bei großen Aggregaten langsam sein

### Best Practices
1. **Snapshots**: Für große Aggregate regelmäßige Snapshots erstellen
2. **Event Versioning**: Schema-Evolution planen
3. **Idempotenz**: Event-Handler idempotent gestalten
4. **Monitoring**: Event Store Performance überwachen
5. **Backup**: Event Store regelmäßig sichern