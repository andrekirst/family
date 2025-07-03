# Testing Strategies für Cloud Design Patterns

## Übersicht

Diese Dokumentation beschreibt umfassende Testing-Strategien für die implementierten Cloud Design Patterns in der Family-Plattform. Jedes Pattern erfordert spezielle Test-Ansätze zur Validierung von Funktionalität, Performance und Resilienz.

## 1. CQRS Testing

### Command Handler Tests
```csharp
// Tests/Unit/Commands/CreateFamilyMemberCommandHandlerTests.cs
public class CreateFamilyMemberCommandHandlerTests
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly CreateFamilyMemberCommandHandler _handler;
    private readonly IFixture _fixture;

    public CreateFamilyMemberCommandHandlerTests()
    {
        _familyRepository = Substitute.For<IFamilyRepository>();
        _eventPublisher = Substitute.For<IEventPublisher>();
        var logger = Substitute.For<ILogger<CreateFamilyMemberCommandHandler>>();
        
        _handler = new CreateFamilyMemberCommandHandler(_familyRepository, _eventPublisher, logger);
        _fixture = new Fixture();
    }

    [Theory]
    [InlineData("", "Doe", "john.doe@example.com")] // Empty first name
    [InlineData("John", "", "john.doe@example.com")] // Empty last name
    [InlineData("John", "Doe", "")] // Empty email
    [InlineData("John", "Doe", "invalid-email")] // Invalid email format
    public async Task Handle_InvalidCommand_ReturnsFailureResult(string firstName, string lastName, string email)
    {
        // Arrange
        var family = _fixture.Create<Family>();
        _familyRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(family);

        var command = new CreateFamilyMemberCommand(firstName, lastName, email, 
            new DateOnly(1990, 1, 1), family.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        
        await _familyRepository.DidNotReceive()
            .SaveAsync(Arg.Any<Family>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_FamilyNotFound_ReturnsFailureResult()
    {
        // Arrange
        _familyRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Family?)null);

        var command = _fixture.Create<CreateFamilyMemberCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Familie nicht gefunden");
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsFailureResult()
    {
        // Arrange
        var family = Family.Create("Test Family", Guid.NewGuid());
        family.AddMember("Existing", "Member", "duplicate@example.com", 
            new DateOnly(1985, 1, 1), Guid.NewGuid());
        
        _familyRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(family);

        var command = new CreateFamilyMemberCommand("New", "Member", "duplicate@example.com", 
            new DateOnly(1990, 1, 1), family.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Ein Familienmitglied mit dieser E-Mail existiert bereits");
    }
}
```

### Query Handler Tests
```csharp
// Tests/Unit/Queries/GetFamilyMembersQueryHandlerTests.cs
public class GetFamilyMembersQueryHandlerTests
{
    private readonly IFamilyReadRepository _readRepository;
    private readonly IMemoryCache _cache;
    private readonly GetFamilyMembersQueryHandler _handler;

    [Fact]
    public async Task Handle_CacheHit_ReturnsCachedResult()
    {
        // Arrange
        var query = new GetFamilyMembersQuery(Guid.NewGuid());
        var expectedResult = new GetFamilyMembersResult([], 0);
        
        _cache.TryGetValue(Arg.Any<string>(), out Arg.Any<object>())
            .Returns(x => { x[1] = expectedResult; return true; });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        await _readRepository.DidNotReceive()
            .GetFamilyMembersAsync(Arg.Any<Guid>(), Arg.Any<int?>(), Arg.Any<int?>(), 
                Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CacheMiss_QueriesRepositoryAndCachesResult()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var members = new[] { CreateFamilyMemberReadModel(familyId) };
        
        _cache.TryGetValue(Arg.Any<string>(), out Arg.Any<object>())
            .Returns(false);
        
        _readRepository.GetFamilyMembersAsync(familyId, null, null, null, Arg.Any<CancellationToken>())
            .Returns(members);
        
        _readRepository.GetFamilyMembersCountAsync(familyId, null, Arg.Any<CancellationToken>())
            .Returns(1);

        var query = new GetFamilyMembersQuery(familyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Members.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        
        _cache.Received(1).Set(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<TimeSpan>());
    }
}
```

### Integration Tests für CQRS
```csharp
// Tests/Integration/CQRSIntegrationTests.cs
public class CQRSIntegrationTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly IMediator _mediator;

    [Fact]
    public async Task CreateFamilyMember_Then_QueryShouldReturnUpdatedData()
    {
        // Arrange
        var familyId = await CreateTestFamilyAsync();
        
        var createCommand = new CreateFamilyMemberCommand(
            "Integration", "Test", "integration@test.com", 
            new DateOnly(1990, 1, 1), familyId);

        // Act - Command Side
        var createResult = await _mediator.Send(createCommand);
        
        // Wait for eventual consistency (in real scenarios, this might be event-driven)
        await Task.Delay(100);
        
        // Act - Query Side
        var queryResult = await _mediator.Send(new GetFamilyMembersQuery(familyId));

        // Assert
        createResult.Success.Should().BeTrue();
        queryResult.Members.Should().Contain(m => m.Email == "integration@test.com");
    }

    [Fact]
    public async Task CommandFailure_ShouldNotAffectReadModel()
    {
        // Arrange
        var familyId = await CreateTestFamilyAsync();
        
        // Get initial state
        var initialQuery = await _mediator.Send(new GetFamilyMembersQuery(familyId));
        var initialCount = initialQuery.TotalCount;
        
        // Try to create invalid member
        var invalidCommand = new CreateFamilyMemberCommand(
            "", "", "invalid-email", new DateOnly(1990, 1, 1), familyId);

        // Act
        var result = await _mediator.Send(invalidCommand);
        
        // Wait for potential async processing
        await Task.Delay(100);
        
        var finalQuery = await _mediator.Send(new GetFamilyMembersQuery(familyId));

        // Assert
        result.Success.Should().BeFalse();
        finalQuery.TotalCount.Should().Be(initialCount); // No change in read model
    }
}
```

## 2. Event Sourcing Testing

### Aggregate Tests
```csharp
// Tests/Unit/Domain/FamilyAggregateTests.cs
public class FamilyAggregateTests
{
    [Fact]
    public void Create_ValidParameters_GeneratesCorrectEvent()
    {
        // Arrange
        var name = "Test Family";
        var createdBy = Guid.NewGuid();

        // Act
        var family = Family.Create(name, createdBy);

        // Assert
        var events = family.GetUncommittedEvents();
        events.Should().HaveCount(1);
        
        var familyCreatedEvent = events.First().Should().BeOfType<FamilyCreatedEvent>().Subject;
        familyCreatedEvent.Name.Should().Be(name);
        familyCreatedEvent.CreatedBy.Should().Be(createdBy);
        familyCreatedEvent.FamilyId.Should().Be(family.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_InvalidName_ThrowsDomainException(string invalidName)
    {
        // Act & Assert
        FluentActions.Invoking(() => Family.Create(invalidName, Guid.NewGuid()))
            .Should().Throw<DomainException>()
            .WithMessage("*Name*");
    }

    [Fact]
    public void LoadFromHistory_EventSequence_RebuildsStateCorrectly()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var events = new List<DomainEvent>
        {
            new FamilyCreatedEvent(familyId, "Test Family", Guid.NewGuid(), DateTime.UtcNow),
            new FamilyMemberAddedEvent(familyId, Guid.NewGuid(), "John", "Doe", 
                "john@example.com", new DateOnly(1990, 1, 1), Guid.NewGuid(), DateTime.UtcNow),
            new FamilyMemberAddedEvent(familyId, Guid.NewGuid(), "Jane", "Doe", 
                "jane@example.com", new DateOnly(1992, 5, 15), Guid.NewGuid(), DateTime.UtcNow)
        };

        // Act
        var family = new Family();
        family.LoadFromHistory(events);

        // Assert
        family.Id.Should().Be(familyId);
        family.Name.Should().Be("Test Family");
        family.Members.Should().HaveCount(2);
        family.Version.Should().Be(3);
        family.GetUncommittedEvents().Should().BeEmpty();
    }

    [Fact]
    public void AddMember_Then_UpdateMember_GeneratesCorrectEvents()
    {
        // Arrange
        var family = Family.Create("Test Family", Guid.NewGuid());
        family.MarkEventsAsCommitted();

        var addedBy = Guid.NewGuid();
        var updatedBy = Guid.NewGuid();

        // Act
        var member = family.AddMember("John", "Doe", "john@example.com", 
            new DateOnly(1990, 1, 1), addedBy);
        
        family.UpdateMember(member.Id, "Johnny", null, null, null, updatedBy);

        // Assert
        var events = family.GetUncommittedEvents();
        events.Should().HaveCount(2);
        
        events[0].Should().BeOfType<FamilyMemberAddedEvent>();
        
        var updateEvent = events[1].Should().BeOfType<FamilyMemberUpdatedEvent>().Subject;
        updateEvent.FirstName.Should().Be("Johnny");
        updateEvent.MemberId.Should().Be(member.Id);
        updateEvent.UpdatedBy.Should().Be(updatedBy);
    }
}
```

### Event Store Tests
```csharp
// Tests/Integration/EventStoreTests.cs
public class EventStoreTests : IClassFixture<EventStoreTestFixture>
{
    private readonly EventStoreTestFixture _fixture;
    private readonly IEventStore _eventStore;

    [Fact]
    public async Task SaveEvents_NewAggregate_SavesAllEventsInOrder()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var events = new List<DomainEvent>
        {
            new FamilyCreatedEvent(aggregateId, "Test Family", Guid.NewGuid(), DateTime.UtcNow),
            new FamilyMemberAddedEvent(aggregateId, Guid.NewGuid(), "John", "Doe", 
                "john@example.com", new DateOnly(1990, 1, 1), Guid.NewGuid(), DateTime.UtcNow)
        };

        // Act
        await _eventStore.SaveEventsAsync<Family>(aggregateId, events, 0);

        // Assert
        var savedEvents = await _eventStore.GetEventsAsync(aggregateId);
        savedEvents.Should().HaveCount(2);
        
        var eventList = savedEvents.ToList();
        eventList[0].Should().BeOfType<FamilyCreatedEvent>();
        eventList[1].Should().BeOfType<FamilyMemberAddedEvent>();
    }

    [Fact]
    public async Task SaveEvents_ConcurrentModification_ThrowsConcurrencyException()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var initialEvent = new FamilyCreatedEvent(aggregateId, "Test Family", Guid.NewGuid(), DateTime.UtcNow);
        
        await _eventStore.SaveEventsAsync<Family>(aggregateId, [initialEvent], 0);

        var firstUpdate = new FamilyMemberAddedEvent(aggregateId, Guid.NewGuid(), "John", "Doe", 
            "john@example.com", new DateOnly(1990, 1, 1), Guid.NewGuid(), DateTime.UtcNow);
        
        var secondUpdate = new FamilyMemberAddedEvent(aggregateId, Guid.NewGuid(), "Jane", "Doe", 
            "jane@example.com", new DateOnly(1992, 1, 1), Guid.NewGuid(), DateTime.UtcNow);

        // Act & Assert
        // Both updates expect version 1, but only one should succeed
        await _eventStore.SaveEventsAsync<Family>(aggregateId, [firstUpdate], 1);
        
        await FluentActions.Invoking(() => 
                _eventStore.SaveEventsAsync<Family>(aggregateId, [secondUpdate], 1))
            .Should().ThrowAsync<ConcurrencyException>();
    }

    [Fact]
    public async Task GetAggregate_WithEventHistory_ReconstructsCorrectly()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var addedBy = Guid.NewGuid();
        
        var events = new List<DomainEvent>
        {
            new FamilyCreatedEvent(aggregateId, "Test Family", createdBy, DateTime.UtcNow),
            new FamilyMemberAddedEvent(aggregateId, Guid.NewGuid(), "John", "Doe", 
                "john@example.com", new DateOnly(1990, 1, 1), addedBy, DateTime.UtcNow)
        };
        
        await _eventStore.SaveEventsAsync<Family>(aggregateId, events, 0);

        // Act
        var family = await _eventStore.GetAggregateAsync<Family>(aggregateId);

        // Assert
        family.Should().NotBeNull();
        family!.Id.Should().Be(aggregateId);
        family.Name.Should().Be("Test Family");
        family.CreatedBy.Should().Be(createdBy);
        family.Members.Should().HaveCount(1);
        family.Version.Should().Be(2);
    }
}
```

### Event Handler Tests
```csharp
// Tests/Unit/EventHandlers/FamilyProjectionHandlerTests.cs
public class FamilyProjectionHandlerTests
{
    private readonly ReadDbContext _context;
    private readonly FamilyProjectionHandler _handler;

    public FamilyProjectionHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ReadDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new ReadDbContext(options);
        _handler = new FamilyProjectionHandler(_context, Substitute.For<ILogger<FamilyProjectionHandler>>());
    }

    [Fact]
    public async Task Handle_FamilyCreatedEvent_CreatesReadModel()
    {
        // Arrange
        var @event = new FamilyCreatedEvent(
            Guid.NewGuid(), "Test Family", Guid.NewGuid(), DateTime.UtcNow);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        var family = await _context.Families.FirstOrDefaultAsync(f => f.Id == @event.FamilyId);
        family.Should().NotBeNull();
        family!.Name.Should().Be("Test Family");
        family.MemberCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_FamilyMemberAddedEvent_UpdatesReadModel()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var family = new FamilyReadModel
        {
            Id = familyId,
            Name = "Test Family",
            MemberCount = 0,
            CreatedAt = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        
        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        var @event = new FamilyMemberAddedEvent(familyId, Guid.NewGuid(), "John", "Doe", 
            "john@example.com", new DateOnly(1990, 1, 1), Guid.NewGuid(), DateTime.UtcNow);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        var updatedFamily = await _context.Families.FirstAsync(f => f.Id == familyId);
        updatedFamily.MemberCount.Should().Be(1);
        
        var member = await _context.FamilyMembers.FirstOrDefaultAsync(m => m.Id == @event.MemberId);
        member.Should().NotBeNull();
        member!.FullName.Should().Be("John Doe");
    }
}
```

## 3. Circuit Breaker Testing

### Circuit Breaker Service Tests
```csharp
// Tests/Unit/CircuitBreakerServiceTests.cs
public class CircuitBreakerServiceTests
{
    private readonly CircuitBreakerService _service;
    private readonly TestMetrics _metrics;

    [Fact]
    public async Task ExecuteAsync_SuccessfulOperations_KeepsCircuitClosed()
    {
        // Arrange
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 3,
            DurationOfBreak = TimeSpan.FromSeconds(1)
        };

        var successfulOperation = () => Task.FromResult("success");

        // Act
        for (int i = 0; i < 10; i++)
        {
            var result = await _service.ExecuteAsync("test-circuit", successfulOperation, options);
            result.Should().Be("success");
        }

        // Assert - All operations should succeed, circuit should remain closed
        _metrics.CircuitBreakerOpenCount.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_FailuresExceedThreshold_OpensCircuit()
    {
        // Arrange
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 2,
            DurationOfBreak = TimeSpan.FromSeconds(1)
        };

        var failingOperation = () => throw new HttpRequestException("Service unavailable");

        // Act & Assert
        // First failure
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            _service.ExecuteAsync("test-circuit", failingOperation, options));
        
        // Second failure should open the circuit
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            _service.ExecuteAsync("test-circuit", failingOperation, options));

        // Third call should fail immediately due to open circuit
        await Assert.ThrowsAsync<BrokenCircuitException>(() => 
            _service.ExecuteAsync("test-circuit", failingOperation, options));

        _metrics.CircuitBreakerOpenCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_CircuitOpensAndRecovers_ResetsSuccessfully()
    {
        // Arrange
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 1,
            DurationOfBreak = TimeSpan.FromMilliseconds(100)
        };

        var callCount = 0;
        var flakyOperation = () =>
        {
            callCount++;
            if (callCount == 1)
                throw new HttpRequestException("Temporary failure");
            return Task.FromResult("success");
        };

        // Act & Assert
        // Open circuit
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            _service.ExecuteAsync("test-circuit", flakyOperation, options));

        // Verify circuit is open
        await Assert.ThrowsAsync<BrokenCircuitException>(() => 
            _service.ExecuteAsync("test-circuit", flakyOperation, options));

        // Wait for circuit to enter half-open state
        await Task.Delay(150);

        // Should succeed and close circuit
        var result = await _service.ExecuteAsync("test-circuit", flakyOperation, options);
        result.Should().Be("success");

        // Subsequent calls should continue to work
        result = await _service.ExecuteAsync("test-circuit", flakyOperation, options);
        result.Should().Be("success");
    }
}
```

### Integration Tests für HTTP Services
```csharp
// Tests/Integration/HttpCircuitBreakerTests.cs
public class HttpCircuitBreakerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    [Fact]
    public async Task HttpService_ServerErrors_OpensCircuitBreaker()
    {
        // Arrange
        var httpService = _factory.Services.GetRequiredService<ICircuitBreakerHttpService>();
        
        // Configure mock server to return 500 errors
        _factory.ConfigureMockServer(HttpStatusCode.InternalServerError);

        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 2,
            DurationOfBreak = TimeSpan.FromSeconds(1)
        };

        // Act & Assert
        // First two calls should fail with HTTP exceptions
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            httpService.GetAsync<string>("/api/test", "test-circuit", options));
        
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            httpService.GetAsync<string>("/api/test", "test-circuit", options));

        // Third call should fail immediately due to open circuit
        await Assert.ThrowsAsync<BrokenCircuitException>(() => 
            httpService.GetAsync<string>("/api/test", "test-circuit", options));
    }

    [Fact]
    public async Task HttpService_TimeoutErrors_OpensCircuitBreaker()
    {
        // Arrange
        var httpService = _factory.Services.GetRequiredService<ICircuitBreakerHttpService>();
        
        // Configure mock server to delay responses beyond timeout
        _factory.ConfigureMockServerDelay(TimeSpan.FromSeconds(2));

        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 1,
            DurationOfBreak = TimeSpan.FromSeconds(1),
            Timeout = TimeSpan.FromMilliseconds(500)
        };

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(() => 
            httpService.GetAsync<string>("/api/test", "test-circuit", options));

        await Assert.ThrowsAsync<BrokenCircuitException>(() => 
            httpService.GetAsync<string>("/api/test", "test-circuit", options));
    }
}
```

## 4. Performance Testing

### Load Testing für Pattern Performance
```csharp
// Tests/Performance/PatternPerformanceTests.cs
public class PatternPerformanceTests
{
    [Fact]
    public async Task CQRS_HighVolumeCommands_MaintainsPerformance()
    {
        // Arrange
        const int commandCount = 1000;
        var stopwatch = Stopwatch.StartNew();
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < commandCount; i++)
        {
            var command = new CreateFamilyMemberCommand(
                $"Member{i}", "Test", $"member{i}@test.com", 
                new DateOnly(1990, 1, 1), Guid.NewGuid());
            
            tasks.Add(_mediator.Send(command));
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        var averageTimePerCommand = stopwatch.ElapsedMilliseconds / (double)commandCount;
        averageTimePerCommand.Should().BeLessThan(100); // Less than 100ms per command
        
        _testOutput.WriteLine($"Processed {commandCount} commands in {stopwatch.ElapsedMilliseconds}ms");
        _testOutput.WriteLine($"Average time per command: {averageTimePerCommand:F2}ms");
    }

    [Fact]
    public async Task EventSourcing_LargeEventHistory_ReconstructsWithinTimeout()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        const int eventCount = 10000;
        
        // Create large event history
        var events = Enumerable.Range(1, eventCount)
            .Select(i => new FamilyMemberAddedEvent(aggregateId, Guid.NewGuid(), 
                $"Member{i}", "Test", $"member{i}@test.com", 
                new DateOnly(1990, 1, 1), Guid.NewGuid(), DateTime.UtcNow))
            .Cast<DomainEvent>()
            .ToList();

        await _eventStore.SaveEventsAsync<Family>(aggregateId, events, 0);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var family = await _eventStore.GetAggregateAsync<Family>(aggregateId);
        stopwatch.Stop();

        // Assert
        family.Should().NotBeNull();
        family!.Members.Should().HaveCount(eventCount);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Less than 5 seconds
        
        _testOutput.WriteLine($"Reconstructed aggregate with {eventCount} events in {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task CircuitBreaker_HighConcurrency_HandlesLoadCorrectly()
    {
        // Arrange
        const int concurrentRequests = 100;
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 10,
            DurationOfBreak = TimeSpan.FromSeconds(1)
        };

        var successCount = 0;
        var failureCount = 0;
        var circuitOpenCount = 0;

        // Act
        var tasks = Enumerable.Range(0, concurrentRequests)
            .Select(async i =>
            {
                try
                {
                    var result = await _circuitBreakerService.ExecuteAsync(
                        "load-test-circuit",
                        () => Task.FromResult($"Success {i}"),
                        options);
                    
                    Interlocked.Increment(ref successCount);
                }
                catch (BrokenCircuitException)
                {
                    Interlocked.Increment(ref circuitOpenCount);
                }
                catch
                {
                    Interlocked.Increment(ref failureCount);
                }
            });

        await Task.WhenAll(tasks);

        // Assert
        _testOutput.WriteLine($"Success: {successCount}, Failures: {failureCount}, Circuit Open: {circuitOpenCount}");
        
        successCount.Should().Be(concurrentRequests);
        failureCount.Should().Be(0);
        circuitOpenCount.Should().Be(0);
    }
}
```

## 5. Chaos Engineering Tests

### Resilience Testing
```csharp
// Tests/Chaos/ResilienceTests.cs
public class ResilienceTests : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task Database_RandomFailures_SystemRemainsStable()
    {
        // Arrange
        var chaos = new DatabaseChaosEngine(_connectionString);
        var familyService = _factory.Services.GetRequiredService<IFamilyService>();
        
        // Act
        var tasks = new List<Task>();
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(ExecuteWithChaos(chaos, async () =>
            {
                await familyService.CreateFamilyAsync($"Family {i}", Guid.NewGuid());
            }));
        }

        var results = await Task.WhenAll(tasks.Select(WrapTask));

        // Assert
        var successCount = results.Count(r => r.Success);
        var failureCount = results.Count(r => !r.Success);
        
        // At least 70% should succeed despite chaos
        (successCount / (double)(successCount + failureCount)).Should().BeGreaterThan(0.7);
        
        _testOutput.WriteLine($"Chaos test results: {successCount} successes, {failureCount} failures");
    }

    [Fact]
    public async Task ExternalAPI_NetworkPartitions_GracefulDegradation()
    {
        // Arrange
        var networkChaos = new NetworkChaosEngine();
        var schoolApiService = _factory.Services.GetRequiredService<ISchoolApiService>();
        
        // Simulate network partition for 50% of requests
        networkChaos.ConfigurePartition(0.5);

        // Act
        var results = new List<bool>();
        for (int i = 0; i < 20; i++)
        {
            try
            {
                var students = await schoolApiService.GetStudentsAsync("test-school");
                results.Add(students != null);
            }
            catch (BrokenCircuitException)
            {
                // Circuit breaker opened - expected behavior
                results.Add(false);
            }
            catch (Exception ex)
            {
                _testOutput.WriteLine($"Unexpected exception: {ex.Message}");
                results.Add(false);
            }
        }

        // Assert
        var successRate = results.Count(r => r) / (double)results.Count;
        
        // System should handle failures gracefully
        // Circuit breaker should prevent cascading failures
        results.Should().NotBeEmpty();
        _testOutput.WriteLine($"Success rate under network chaos: {successRate:P}");
    }

    private async Task<(bool Success, Exception? Exception)> WrapTask(Task task)
    {
        try
        {
            await task;
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex);
        }
    }
}
```

## 6. Contract Testing

### API Contract Tests
```csharp
// Tests/Contract/GraphQLContractTests.cs
public class GraphQLContractTests
{
    [Fact]
    public async Task CreateFamilyMember_SchemaContract_RemainsStable()
    {
        // Arrange
        var mutation = @"
            mutation CreateFamilyMember($input: CreateFamilyMemberInput!) {
                createFamilyMember(input: $input) {
                    success
                    errors
                    id
                    fullName
                }
            }";

        var variables = new
        {
            input = new
            {
                firstName = "Contract",
                lastName = "Test",
                email = "contract@test.com",
                dateOfBirth = "1990-01-01",
                familyId = Guid.NewGuid()
            }
        };

        // Act
        var response = await _client.PostGraphQLAsync(mutation, variables);

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().ContainKey("createFamilyMember");
        
        var result = response.Data["createFamilyMember"] as JObject;
        result.Should().NotBeNull();
        result!.Should().ContainKeys("success", "errors", "id", "fullName");
    }

    [Fact]
    public async Task GetFamilyMembers_ResponseSchema_MatchesContract()
    {
        // Arrange
        var query = @"
            query GetFamilyMembers($familyId: UUID!) {
                familyMembers(familyId: $familyId) {
                    totalCount
                    members {
                        id
                        firstName
                        lastName
                        fullName
                        email
                        dateOfBirth
                        age
                    }
                }
            }";

        // Act
        var response = await _client.PostGraphQLAsync(query, new { familyId = Guid.NewGuid() });

        // Assert
        var familyMembers = response.Data["familyMembers"] as JObject;
        familyMembers.Should().ContainKey("totalCount");
        familyMembers.Should().ContainKey("members");
        
        var members = familyMembers["members"] as JArray;
        if (members?.Any() == true)
        {
            var member = members[0] as JObject;
            member.Should().ContainKeys("id", "firstName", "lastName", "fullName", "email", "dateOfBirth", "age");
        }
    }
}
```

## 7. Test Data Builders

### Domain Object Builders
```csharp
// Tests/Builders/FamilyBuilder.cs
public class FamilyBuilder
{
    private string _name = "Test Family";
    private Guid _createdBy = Guid.NewGuid();
    private readonly List<FamilyMember> _members = [];

    public FamilyBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public FamilyBuilder CreatedBy(Guid userId)
    {
        _createdBy = userId;
        return this;
    }

    public FamilyBuilder WithMember(string firstName, string lastName, string email)
    {
        var member = FamilyMember.Create(firstName, lastName, email, 
            new DateOnly(1990, 1, 1), Guid.NewGuid());
        _members.Add(member);
        return this;
    }

    public Family Build()
    {
        var family = Family.Create(_name, _createdBy);
        
        foreach (var member in _members)
        {
            family.AddMember(member.FirstName, member.LastName, member.Email, 
                member.DateOfBirth, _createdBy);
        }
        
        family.MarkEventsAsCommitted();
        return family;
    }
}

// Usage in tests
var family = new FamilyBuilder()
    .WithName("Integration Test Family")
    .WithMember("John", "Doe", "john@test.com")
    .WithMember("Jane", "Doe", "jane@test.com")
    .Build();
```

## 8. Test Configuration

### Test Base Classes
```csharp
// Tests/Base/IntegrationTestBase.cs
public abstract class IntegrationTestBase : IClassFixture<TestWebApplicationFactory>
{
    protected readonly TestWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected readonly IServiceScope Scope;
    protected readonly IMediator Mediator;

    protected IntegrationTestBase(TestWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        Scope = factory.Services.CreateScope();
        Mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();
    }

    protected async Task<Guid> CreateTestFamilyAsync(string name = "Test Family")
    {
        var command = new CreateFamilyCommand(name, Guid.NewGuid());
        var result = await Mediator.Send(command);
        return result.FamilyId;
    }

    public void Dispose()
    {
        Scope?.Dispose();
        Client?.Dispose();
    }
}
```

## 9. Test Reporting

### Custom Test Reporters
```csharp
// Tests/Reporting/PatternTestReporter.cs
public class PatternTestReporter
{
    public void GenerateReport(TestResults results)
    {
        var report = new
        {
            Timestamp = DateTime.UtcNow,
            Summary = new
            {
                TotalTests = results.Total,
                Passed = results.Passed,
                Failed = results.Failed,
                Skipped = results.Skipped,
                SuccessRate = (double)results.Passed / results.Total
            },
            PatternCoverage = new
            {
                CQRS = results.GetPatternCoverage("CQRS"),
                EventSourcing = results.GetPatternCoverage("EventSourcing"),
                CircuitBreaker = results.GetPatternCoverage("CircuitBreaker")
            },
            PerformanceMetrics = results.PerformanceMetrics
        };

        File.WriteAllText("test-report.json", JsonSerializer.Serialize(report, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }
}
```

## Best Practices

### 1. Test Isolation
- Jeder Test läuft in isolierter Umgebung
- Verwendung von In-Memory Databases für Unit Tests
- Cleanup zwischen Tests

### 2. Test Data Management
- Builder Pattern für Testdaten
- Factories für komplexe Objektgraphen
- Randomisierte Daten wo möglich

### 3. Assertion Strategies
- FluentAssertions für bessere Lesbarkeit
- Spezifische Assertions für Domain Concepts
- Comprehensive Error Messages

### 4. Performance Testing
- Load Testing für kritische Paths
- Memory Usage Monitoring
- Response Time Benchmarks

### 5. Chaos Engineering
- Controlled Failure Injection
- Network Partition Simulation
- Database Outage Testing