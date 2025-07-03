# CQRS (Command Query Responsibility Segregation) - Implementation Guide

## Übersicht

CQRS trennt Lese- und Schreiboperationen in separate Modelle für bessere Performance und Skalierbarkeit. In der Family-Plattform nutzen wir CQRS für die Trennung von GraphQL Mutations (Commands) und Queries.

## Architektur-Überblick

```
┌─────────────────┐    ┌─────────────────┐
│   GraphQL       │    │   GraphQL       │
│   Mutations     │    │   Queries       │
│   (Commands)    │    │   (Queries)     │
└─────────┬───────┘    └─────────┬───────┘
          │                      │
          ▼                      ▼
┌─────────────────┐    ┌─────────────────┐
│  Command        │    │  Query          │
│  Handlers       │    │  Handlers       │
└─────────┬───────┘    └─────────┬───────┘
          │                      │
          ▼                      ▼
┌─────────────────┐    ┌─────────────────┐
│  Write Model    │    │  Read Model     │
│  (Domain)       │    │  (Projections)  │
└─────────────────┘    └─────────────────┘
          │                      ▲
          │       Events         │
          └──────────────────────┘
```

## 1. Command-Seite (Schreiboperationen)

### Command Definition
```csharp
// Commands/CreateFamilyMemberCommand.cs
public record CreateFamilyMemberCommand(
    string FirstName,
    string LastName,
    string Email,
    DateOnly DateOfBirth,
    Guid FamilyId
) : ICommand<CreateFamilyMemberResult>;

public record CreateFamilyMemberResult(
    Guid Id,
    string FullName,
    bool Success,
    IEnumerable<string> Errors
);
```

### Command Handler
```csharp
// Commands/Handlers/CreateFamilyMemberCommandHandler.cs
public class CreateFamilyMemberCommandHandler 
    : ICommandHandler<CreateFamilyMemberCommand, CreateFamilyMemberResult>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<CreateFamilyMemberCommandHandler> _logger;

    public CreateFamilyMemberCommandHandler(
        IFamilyRepository familyRepository,
        IEventPublisher eventPublisher,
        ILogger<CreateFamilyMemberCommandHandler> logger)
    {
        _familyRepository = familyRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<CreateFamilyMemberResult> Handle(
        CreateFamilyMemberCommand command, 
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Load Family Aggregate
            var family = await _familyRepository.GetByIdAsync(command.FamilyId, cancellationToken);
            if (family == null)
            {
                return new CreateFamilyMemberResult(
                    Guid.Empty, 
                    string.Empty, 
                    false, 
                    ["Familie nicht gefunden"]);
            }

            // 2. Create Family Member through Domain Logic
            var familyMember = family.AddMember(
                command.FirstName,
                command.LastName,
                command.Email,
                command.DateOfBirth);

            // 3. Save Changes (triggers Domain Events)
            await _familyRepository.SaveAsync(family, cancellationToken);

            // 4. Publish Domain Events
            foreach (var domainEvent in family.GetUncommittedEvents())
            {
                await _eventPublisher.PublishAsync(domainEvent, cancellationToken);
            }

            family.MarkEventsAsCommitted();

            _logger.LogInformation(
                "Family member created: {MemberId} for Family {FamilyId}",
                familyMember.Id, command.FamilyId);

            return new CreateFamilyMemberResult(
                familyMember.Id,
                familyMember.FullName,
                true,
                []);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain error creating family member");
            return new CreateFamilyMemberResult(
                Guid.Empty, 
                string.Empty, 
                false, 
                [ex.Message]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating family member");
            return new CreateFamilyMemberResult(
                Guid.Empty, 
                string.Empty, 
                false, 
                ["Ein unerwarteter Fehler ist aufgetreten"]);
        }
    }
}
```

### Domain Model (Write-Side)
```csharp
// Domain/Family.cs
public class Family : AggregateRoot
{
    private readonly List<FamilyMember> _members = [];
    
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public IReadOnlyList<FamilyMember> Members => _members.AsReadOnly();

    private Family() { } // For EF

    public static Family Create(string name, Guid createdBy)
    {
        var family = new Family
        {
            Id = Guid.NewGuid(),
            Name = name
        };

        family.AddDomainEvent(new FamilyCreatedEvent(family.Id, name, createdBy));
        return family;
    }

    public FamilyMember AddMember(string firstName, string lastName, string email, DateOnly dateOfBirth)
    {
        // Business Logic Validation
        if (_members.Any(m => m.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException("Ein Familienmitglied mit dieser E-Mail existiert bereits");

        var member = FamilyMember.Create(firstName, lastName, email, dateOfBirth, Id);
        _members.Add(member);

        AddDomainEvent(new FamilyMemberAddedEvent(
            Id, 
            member.Id, 
            member.FullName, 
            member.Email));

        return member;
    }
}

// Domain/FamilyMember.cs
public class FamilyMember : Entity
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public DateOnly DateOfBirth { get; private set; }
    public Guid FamilyId { get; private set; }
    
    public string FullName => $"{FirstName} {LastName}";

    private FamilyMember() { } // For EF

    public static FamilyMember Create(
        string firstName, 
        string lastName, 
        string email, 
        DateOnly dateOfBirth, 
        Guid familyId)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("Vorname ist erforderlich");
        
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Nachname ist erforderlich");

        if (!IsValidEmail(email))
            throw new DomainException("Ungültige E-Mail-Adresse");

        return new FamilyMember
        {
            Id = Guid.NewGuid(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            DateOfBirth = dateOfBirth,
            FamilyId = familyId
        };
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
```

## 2. Query-Seite (Leseoperationen)

### Query Definition
```csharp
// Queries/GetFamilyMembersQuery.cs
public record GetFamilyMembersQuery(
    Guid FamilyId,
    int? Skip = null,
    int? Take = null,
    string? SearchTerm = null
) : IQuery<GetFamilyMembersResult>;

public record GetFamilyMembersResult(
    IEnumerable<FamilyMemberDto> Members,
    int TotalCount
);

public record FamilyMemberDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    DateOnly DateOfBirth,
    int Age,
    Guid FamilyId
);
```

### Query Handler
```csharp
// Queries/Handlers/GetFamilyMembersQueryHandler.cs
public class GetFamilyMembersQueryHandler 
    : IQueryHandler<GetFamilyMembersQuery, GetFamilyMembersResult>
{
    private readonly IFamilyReadRepository _readRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GetFamilyMembersQueryHandler> _logger;

    public GetFamilyMembersQueryHandler(
        IFamilyReadRepository readRepository,
        IMemoryCache cache,
        ILogger<GetFamilyMembersQueryHandler> logger)
    {
        _readRepository = readRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<GetFamilyMembersResult> Handle(
        GetFamilyMembersQuery query, 
        CancellationToken cancellationToken)
    {
        var cacheKey = $"family_members_{query.FamilyId}_{query.Skip}_{query.Take}_{query.SearchTerm}";

        if (_cache.TryGetValue(cacheKey, out GetFamilyMembersResult cachedResult))
        {
            _logger.LogDebug("Cache hit for family members query: {CacheKey}", cacheKey);
            return cachedResult;
        }

        var members = await _readRepository.GetFamilyMembersAsync(
            query.FamilyId,
            query.Skip,
            query.Take,
            query.SearchTerm,
            cancellationToken);

        var totalCount = await _readRepository.GetFamilyMembersCountAsync(
            query.FamilyId,
            query.SearchTerm,
            cancellationToken);

        var memberDtos = members.Select(m => new FamilyMemberDto(
            m.Id,
            m.FirstName,
            m.LastName,
            m.FullName,
            m.Email,
            m.DateOfBirth,
            CalculateAge(m.DateOfBirth),
            m.FamilyId)).ToList();

        var result = new GetFamilyMembersResult(memberDtos, totalCount);

        // Cache for 5 minutes
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        _logger.LogDebug("Cache miss for family members query: {CacheKey}", cacheKey);
        return result;
    }

    private static int CalculateAge(DateOnly birthDate)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - birthDate.Year;
        if (birthDate > today.AddYears(-age)) age--;
        return age;
    }
}
```

### Read Model Repository
```csharp
// Infrastructure/Repositories/FamilyReadRepository.cs
public class FamilyReadRepository : IFamilyReadRepository
{
    private readonly ReadDbContext _context;
    private readonly ILogger<FamilyReadRepository> _logger;

    public FamilyReadRepository(ReadDbContext context, ILogger<FamilyReadRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<FamilyMemberReadModel>> GetFamilyMembersAsync(
        Guid familyId,
        int? skip,
        int? take,
        string? searchTerm,
        CancellationToken cancellationToken)
    {
        var query = _context.FamilyMembers
            .Where(fm => fm.FamilyId == familyId)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLowerInvariant();
            query = query.Where(fm => 
                fm.FirstName.ToLower().Contains(term) ||
                fm.LastName.ToLower().Contains(term) ||
                fm.Email.ToLower().Contains(term));
        }

        query = query.OrderBy(fm => fm.LastName).ThenBy(fm => fm.FirstName);

        if (skip.HasValue)
            query = query.Skip(skip.Value);

        if (take.HasValue)
            query = query.Take(take.Value);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<int> GetFamilyMembersCountAsync(
        Guid familyId,
        string? searchTerm,
        CancellationToken cancellationToken)
    {
        var query = _context.FamilyMembers
            .Where(fm => fm.FamilyId == familyId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLowerInvariant();
            query = query.Where(fm => 
                fm.FirstName.ToLower().Contains(term) ||
                fm.LastName.ToLower().Contains(term) ||
                fm.Email.ToLower().Contains(term));
        }

        return await query.CountAsync(cancellationToken);
    }
}
```

## 3. GraphQL Integration

### Mutations (Commands)
```csharp
// GraphQL/Mutations/FamilyMutations.cs
[ExtendObjectType<Mutation>]
public class FamilyMutations
{
    public async Task<CreateFamilyMemberResult> CreateFamilyMemberAsync(
        CreateFamilyMemberInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateFamilyMemberCommand(
            input.FirstName,
            input.LastName,
            input.Email,
            input.DateOfBirth,
            input.FamilyId);

        return await mediator.Send(command, cancellationToken);
    }
}

public record CreateFamilyMemberInput(
    string FirstName,
    string LastName,
    string Email,
    DateOnly DateOfBirth,
    Guid FamilyId
);
```

### Queries (Read-Side)
```csharp
// GraphQL/Queries/FamilyQueries.cs
[ExtendObjectType<Query>]
public class FamilyQueries
{
    public async Task<GetFamilyMembersResult> GetFamilyMembersAsync(
        Guid familyId,
        int? skip,
        int? take,
        string? searchTerm,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetFamilyMembersQuery(familyId, skip, take, searchTerm);
        return await mediator.Send(query, cancellationToken);
    }
}
```

## 4. MediatR Configuration

### Service Registration
```csharp
// Program.cs
services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssemblyContaining<CreateFamilyMemberCommandHandler>();
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
});

// Separate DbContexts for Read/Write
services.AddDbContext<WriteDbContext>(options =>
    options.UseNpgsql(connectionString));

services.AddDbContext<ReadDbContext>(options =>
    options.UseNpgsql(connectionString));
```

### Command/Query Interfaces
```csharp
// Abstractions/ICommand.cs
public interface ICommand<TResponse> : IRequest<TResponse> { }

public interface ICommandHandler<TCommand, TResponse> 
    : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse> { }

// Abstractions/IQuery.cs
public interface IQuery<TResponse> : IRequest<TResponse> { }

public interface IQueryHandler<TQuery, TResponse> 
    : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse> { }
```

## 5. Testing

### Command Handler Tests
```csharp
// Tests/Unit/CreateFamilyMemberCommandHandlerTests.cs
public class CreateFamilyMemberCommandHandlerTests
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly CreateFamilyMemberCommandHandler _handler;

    public CreateFamilyMemberCommandHandlerTests()
    {
        _familyRepository = Substitute.For<IFamilyRepository>();
        _eventPublisher = Substitute.For<IEventPublisher>();
        var logger = Substitute.For<ILogger<CreateFamilyMemberCommandHandler>>();
        
        _handler = new CreateFamilyMemberCommandHandler(
            _familyRepository, _eventPublisher, logger);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesAndReturnsFamilyMember()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = Family.Create("Test Family", Guid.NewGuid());
        
        _familyRepository.GetByIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns(family);

        var command = new CreateFamilyMemberCommand(
            "John", "Doe", "john.doe@example.com", 
            new DateOnly(1990, 1, 1), familyId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.FullName.Should().Be("John Doe");
        
        await _familyRepository.Received(1)
            .SaveAsync(family, Arg.Any<CancellationToken>());
        
        await _eventPublisher.Received()
            .PublishAsync(Arg.Any<FamilyMemberAddedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_FamilyNotFound_ReturnsError()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        _familyRepository.GetByIdAsync(familyId, Arg.Any<CancellationToken>())
            .Returns((Family)null);

        var command = new CreateFamilyMemberCommand(
            "John", "Doe", "john.doe@example.com", 
            new DateOnly(1990, 1, 1), familyId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Familie nicht gefunden");
    }
}
```

### Query Handler Tests
```csharp
// Tests/Unit/GetFamilyMembersQueryHandlerTests.cs
public class GetFamilyMembersQueryHandlerTests
{
    private readonly IFamilyReadRepository _readRepository;
    private readonly IMemoryCache _cache;
    private readonly GetFamilyMembersQueryHandler _handler;

    public GetFamilyMembersQueryHandlerTests()
    {
        _readRepository = Substitute.For<IFamilyReadRepository>();
        _cache = Substitute.For<IMemoryCache>();
        var logger = Substitute.For<ILogger<GetFamilyMembersQueryHandler>>();
        
        _handler = new GetFamilyMembersQueryHandler(_readRepository, _cache, logger);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsMembers()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var members = new[]
        {
            new FamilyMemberReadModel
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                DateOfBirth = new DateOnly(1990, 1, 1),
                FamilyId = familyId
            }
        };

        _readRepository.GetFamilyMembersAsync(
                familyId, null, null, null, Arg.Any<CancellationToken>())
            .Returns(members);
        
        _readRepository.GetFamilyMembersCountAsync(
                familyId, null, Arg.Any<CancellationToken>())
            .Returns(1);

        var query = new GetFamilyMembersQuery(familyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Members.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Members.First().FullName.Should().Be("John Doe");
    }
}
```

## 6. Performance Considerations

### Caching Strategy
- Query-Results werden für 5 Minuten gecacht
- Cache-Invalidation bei Commands über Events
- Separate Cache-Keys für verschiedene Query-Parameter

### Read Model Optimization
- Separate Read-Database für optimierte Queries
- Denormalisierte Daten für bessere Performance
- Indizierung für häufige Query-Patterns

### Event-Driven Updates
- Read Models werden asynchron über Events aktualisiert
- Eventual Consistency zwischen Write- und Read-Side
- Kompensierung durch Cache-Strategien

## Vorteile der CQRS-Implementierung

1. **Performance**: Optimierte Read- und Write-Models
2. **Skalierbarkeit**: Unabhängige Skalierung von Lese- und Schreiboperationen
3. **Flexibilität**: Verschiedene Datenmodelle für verschiedene Anwendungsfälle
4. **Testbarkeit**: Klare Trennung von Business Logic und Queries
5. **Caching**: Effektive Caching-Strategien für Read-Side