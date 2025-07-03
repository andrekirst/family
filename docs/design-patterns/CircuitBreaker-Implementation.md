# Circuit Breaker Pattern - Implementation Guide

## Übersicht

Der Circuit Breaker Pattern verhindert, dass eine Anwendung wiederholt versucht, eine Operation auszuführen, die wahrscheinlich fehlschlägt. Er überwacht Fehler und "öffnet" den Circuit, um weitere Aufrufe zu verhindern, bis der Service wieder verfügbar ist.

## Circuit Breaker Zustände

```
         ┌─────────────┐
         │   CLOSED    │◀──────────┐
         │  (Normal)   │           │
         └─────┬───────┘           │
               │ Failures          │
               │ ≥ Threshold       │ Success
               ▼                   │ ≥ Threshold
         ┌─────────────┐           │
         │    OPEN     │           │
         │ (Blocking)  │           │
         └─────┬───────┘           │
               │ Timeout           │
               │ Expired           │
               ▼                   │
         ┌─────────────┐           │
         │ HALF-OPEN   │───────────┘
         │ (Testing)   │
         └─────────────┘
```

**CLOSED**: Normaler Betrieb, alle Requests werden durchgelassen  
**OPEN**: Circuit ist "offen", alle Requests werden sofort abgelehnt  
**HALF-OPEN**: Test-Modus, begrenzte Anzahl von Requests wird durchgelassen

## 1. Circuit Breaker Implementation mit Polly

### Base Circuit Breaker Service
```csharp
// Infrastructure/CircuitBreaker/CircuitBreakerService.cs
public class CircuitBreakerService : ICircuitBreakerService
{
    private readonly Dictionary<string, IAsyncPolicy> _circuitBreakers = new();
    private readonly ILogger<CircuitBreakerService> _logger;
    private readonly IConfiguration _configuration;

    public CircuitBreakerService(
        ILogger<CircuitBreakerService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public IAsyncPolicy GetCircuitBreaker(string name, CircuitBreakerOptions? options = null)
    {
        if (_circuitBreakers.TryGetValue(name, out var existingPolicy))
        {
            return existingPolicy;
        }

        options ??= GetDefaultOptions(name);
        var policy = CreateCircuitBreakerPolicy(name, options);
        _circuitBreakers[name] = policy;
        
        return policy;
    }

    public async Task<T> ExecuteAsync<T>(
        string circuitBreakerName,
        Func<Task<T>> operation,
        CircuitBreakerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var circuitBreaker = GetCircuitBreaker(circuitBreakerName, options);
        return await circuitBreaker.ExecuteAsync(operation);
    }

    public async Task ExecuteAsync(
        string circuitBreakerName,
        Func<Task> operation,
        CircuitBreakerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var circuitBreaker = GetCircuitBreaker(circuitBreakerName, options);
        await circuitBreaker.ExecuteAsync(operation);
    }

    private IAsyncPolicy CreateCircuitBreakerPolicy(string name, CircuitBreakerOptions options)
    {
        return Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .Or<TaskCanceledException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: options.FailureThreshold,
                durationOfBreak: options.DurationOfBreak,
                onBreak: (exception, duration) =>
                {
                    _logger.LogWarning(
                        "Circuit breaker '{CircuitBreakerName}' opened for {Duration}ms due to: {Exception}",
                        name, duration.TotalMilliseconds, exception);
                },
                onReset: () =>
                {
                    _logger.LogInformation(
                        "Circuit breaker '{CircuitBreakerName}' reset to closed state", name);
                },
                onHalfOpen: () =>
                {
                    _logger.LogInformation(
                        "Circuit breaker '{CircuitBreakerName}' entered half-open state", name);
                });
    }

    private CircuitBreakerOptions GetDefaultOptions(string name)
    {
        var section = _configuration.GetSection($"CircuitBreaker:{name}");
        if (section.Exists())
        {
            return section.Get<CircuitBreakerOptions>() ?? new CircuitBreakerOptions();
        }

        return new CircuitBreakerOptions();
    }
}

// Configuration/CircuitBreakerOptions.cs
public class CircuitBreakerOptions
{
    public int FailureThreshold { get; set; } = 5;
    public TimeSpan DurationOfBreak { get; set; } = TimeSpan.FromSeconds(30);
    public int SuccessThreshold { get; set; } = 3;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
}

// Abstractions/ICircuitBreakerService.cs
public interface ICircuitBreakerService
{
    IAsyncPolicy GetCircuitBreaker(string name, CircuitBreakerOptions? options = null);
    
    Task<T> ExecuteAsync<T>(
        string circuitBreakerName,
        Func<Task<T>> operation,
        CircuitBreakerOptions? options = null,
        CancellationToken cancellationToken = default);
    
    Task ExecuteAsync(
        string circuitBreakerName,
        Func<Task> operation,
        CircuitBreakerOptions? options = null,
        CancellationToken cancellationToken = default);
}
```

### Enhanced Circuit Breaker with Retry and Timeout
```csharp
// Infrastructure/CircuitBreaker/EnhancedCircuitBreakerService.cs
public class EnhancedCircuitBreakerService : IEnhancedCircuitBreakerService
{
    private readonly Dictionary<string, IAsyncPolicy> _policies = new();
    private readonly ILogger<EnhancedCircuitBreakerService> _logger;
    private readonly IMetrics _metrics;

    public EnhancedCircuitBreakerService(
        ILogger<EnhancedCircuitBreakerService> logger,
        IMetrics metrics)
    {
        _logger = logger;
        _metrics = metrics;
    }

    public IAsyncPolicy GetEnhancedPolicy(string name, EnhancedCircuitBreakerOptions options)
    {
        if (_policies.TryGetValue(name, out var existingPolicy))
        {
            return existingPolicy;
        }

        var policy = CreateEnhancedPolicy(name, options);
        _policies[name] = policy;
        
        return policy;
    }

    private IAsyncPolicy CreateEnhancedPolicy(string name, EnhancedCircuitBreakerOptions options)
    {
        // Timeout Policy
        var timeoutPolicy = Policy.TimeoutAsync(
            options.Timeout,
            TimeoutStrategy.Optimistic,
            onTimeoutAsync: (context, timeout, task) =>
            {
                _logger.LogWarning(
                    "Operation '{OperationName}' timed out after {Timeout}ms",
                    name, timeout.TotalMilliseconds);
                
                _metrics.IncrementCounter("circuit_breaker_timeout", [
                    ("operation", name),
                    ("reason", "timeout")
                ]);
                
                return Task.CompletedTask;
            });

        // Retry Policy with Exponential Backoff
        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: options.RetryCount,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + // Exponential backoff
                    TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000)), // Jitter
                onRetry: (outcome, delay, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Retry {RetryCount}/{MaxRetries} for '{OperationName}' after {Delay}ms. Exception: {Exception}",
                        retryCount, options.RetryCount, name, delay.TotalMilliseconds, outcome.Exception?.Message);
                    
                    _metrics.IncrementCounter("circuit_breaker_retry", [
                        ("operation", name),
                        ("retry_attempt", retryCount.ToString())
                    ]);
                });

        // Circuit Breaker Policy
        var circuitBreakerPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .Or<TaskCanceledException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: options.FailureThreshold,
                durationOfBreak: options.DurationOfBreak,
                onBreak: (exception, duration) =>
                {
                    _logger.LogError(
                        "Circuit breaker '{CircuitBreakerName}' OPENED for {Duration}ms. Exception: {Exception}",
                        name, duration.TotalMilliseconds, exception.Exception?.Message ?? exception.Result?.ToString());
                    
                    _metrics.IncrementCounter("circuit_breaker_opened", [
                        ("operation", name),
                        ("duration", duration.TotalSeconds.ToString())
                    ]);
                },
                onReset: () =>
                {
                    _logger.LogInformation(
                        "Circuit breaker '{CircuitBreakerName}' RESET to closed state", name);
                    
                    _metrics.IncrementCounter("circuit_breaker_closed", [
                        ("operation", name)
                    ]);
                },
                onHalfOpen: () =>
                {
                    _logger.LogInformation(
                        "Circuit breaker '{CircuitBreakerName}' HALF-OPENED", name);
                    
                    _metrics.IncrementCounter("circuit_breaker_half_open", [
                        ("operation", name)
                    ]);
                });

        // Fallback Policy
        var fallbackPolicy = Policy
            .Handle<BrokenCircuitException>()
            .Or<CircuitBreakerOpenException>()
            .FallbackAsync(
                fallbackAction: async (cancellationToken) =>
                {
                    _logger.LogWarning(
                        "Circuit breaker '{CircuitBreakerName}' is open, executing fallback", name);
                    
                    _metrics.IncrementCounter("circuit_breaker_fallback", [
                        ("operation", name)
                    ]);
                    
                    if (options.FallbackAction != null)
                    {
                        await options.FallbackAction();
                    }
                },
                onFallback: (exception) =>
                {
                    _logger.LogInformation(
                        "Fallback executed for '{OperationName}' due to: {Exception}",
                        name, exception.Exception?.Message);
                    
                    return Task.CompletedTask;
                });

        // Combine all policies: Fallback -> Circuit Breaker -> Retry -> Timeout
        return Policy.WrapAsync(fallbackPolicy, circuitBreakerPolicy, retryPolicy, timeoutPolicy);
    }
}

public class EnhancedCircuitBreakerOptions : CircuitBreakerOptions
{
    public int RetryCount { get; set; } = 3;
    public Func<Task>? FallbackAction { get; set; }
    public bool UseJitter { get; set; } = true;
}
```

## 2. HTTP Client Integration

### Circuit Breaker HTTP Service
```csharp
// Infrastructure/Http/CircuitBreakerHttpService.cs
public class CircuitBreakerHttpService : ICircuitBreakerHttpService
{
    private readonly HttpClient _httpClient;
    private readonly ICircuitBreakerService _circuitBreakerService;
    private readonly ILogger<CircuitBreakerHttpService> _logger;

    public CircuitBreakerHttpService(
        HttpClient httpClient,
        ICircuitBreakerService circuitBreakerService,
        ILogger<CircuitBreakerHttpService> logger)
    {
        _httpClient = httpClient;
        _circuitBreakerService = circuitBreakerService;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(
        string endpoint,
        string circuitBreakerName,
        CircuitBreakerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return await _circuitBreakerService.ExecuteAsync(
            circuitBreakerName,
            async () =>
            {
                _logger.LogDebug("Making GET request to {Endpoint}", endpoint);
                
                var response = await _httpClient.GetAsync(endpoint, cancellationToken);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            },
            options,
            cancellationToken);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request,
        string circuitBreakerName,
        CircuitBreakerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return await _circuitBreakerService.ExecuteAsync(
            circuitBreakerName,
            async () =>
            {
                _logger.LogDebug("Making POST request to {Endpoint}", endpoint);
                
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
                
                var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<TResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            },
            options,
            cancellationToken);
    }
}
```

### External API Service mit Circuit Breaker
```csharp
// Application/Services/SchoolApiService.cs
public class SchoolApiService : ISchoolApiService
{
    private readonly ICircuitBreakerHttpService _httpService;
    private readonly ILogger<SchoolApiService> _logger;
    private readonly SchoolApiOptions _options;

    private static readonly CircuitBreakerOptions CircuitBreakerOptions = new()
    {
        FailureThreshold = 3,
        DurationOfBreak = TimeSpan.FromMinutes(2),
        Timeout = TimeSpan.FromSeconds(15)
    };

    public SchoolApiService(
        ICircuitBreakerHttpService httpService,
        IOptions<SchoolApiOptions> options,
        ILogger<SchoolApiService> logger)
    {
        _httpService = httpService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IEnumerable<Student>?> GetStudentsAsync(
        string schoolId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching students for school {SchoolId}", schoolId);
            
            var students = await _httpService.GetAsync<IEnumerable<Student>>(
                $"{_options.BaseUrl}/schools/{schoolId}/students",
                "school-api-students",
                CircuitBreakerOptions,
                cancellationToken);

            _logger.LogInformation("Successfully fetched {StudentCount} students", students?.Count() ?? 0);
            return students;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogWarning("School API circuit breaker is open: {Message}", ex.Message);
            
            // Return cached data or default fallback
            return await GetCachedStudentsAsync(schoolId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching students for school {SchoolId}", schoolId);
            throw;
        }
    }

    public async Task<StudentGrades?> GetStudentGradesAsync(
        string schoolId,
        string studentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching grades for student {StudentId} in school {SchoolId}", 
                studentId, schoolId);
            
            var grades = await _httpService.GetAsync<StudentGrades>(
                $"{_options.BaseUrl}/schools/{schoolId}/students/{studentId}/grades",
                "school-api-grades",
                CircuitBreakerOptions,
                cancellationToken);

            return grades;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogWarning("School API circuit breaker is open for grades: {Message}", ex.Message);
            
            // Return cached grades or indicate service unavailable
            return await GetCachedGradesAsync(schoolId, studentId, cancellationToken);
        }
    }

    private async Task<IEnumerable<Student>?> GetCachedStudentsAsync(
        string schoolId, 
        CancellationToken cancellationToken)
    {
        // Implementation would check Redis cache or local storage
        _logger.LogInformation("Returning cached students for school {SchoolId}", schoolId);
        return await Task.FromResult<IEnumerable<Student>?>(null);
    }

    private async Task<StudentGrades?> GetCachedGradesAsync(
        string schoolId, 
        string studentId, 
        CancellationToken cancellationToken)
    {
        // Implementation would check Redis cache
        _logger.LogInformation("Returning cached grades for student {StudentId}", studentId);
        return await Task.FromResult<StudentGrades?>(null);
    }
}

public class SchoolApiOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
```

## 3. Database Circuit Breaker

### Database Service mit Circuit Breaker
```csharp
// Infrastructure/Database/CircuitBreakerDbService.cs
public class CircuitBreakerDbService : ICircuitBreakerDbService
{
    private readonly IDbContextFactory<FamilyDbContext> _contextFactory;
    private readonly ICircuitBreakerService _circuitBreakerService;
    private readonly ILogger<CircuitBreakerDbService> _logger;

    private static readonly CircuitBreakerOptions DatabaseOptions = new()
    {
        FailureThreshold = 5,
        DurationOfBreak = TimeSpan.FromMinutes(1),
        Timeout = TimeSpan.FromSeconds(30)
    };

    public CircuitBreakerDbService(
        IDbContextFactory<FamilyDbContext> contextFactory,
        ICircuitBreakerService circuitBreakerService,
        ILogger<CircuitBreakerDbService> logger)
    {
        _contextFactory = contextFactory;
        _circuitBreakerService = circuitBreakerService;
        _logger = logger;
    }

    public async Task<T?> ExecuteQueryAsync<T>(
        Func<FamilyDbContext, Task<T>> query,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        return await _circuitBreakerService.ExecuteAsync(
            $"database-{operationName}",
            async () =>
            {
                await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
                return await query(context);
            },
            DatabaseOptions,
            cancellationToken);
    }

    public async Task ExecuteCommandAsync(
        Func<FamilyDbContext, Task> command,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        await _circuitBreakerService.ExecuteAsync(
            $"database-{operationName}",
            async () =>
            {
                await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
                await command(context);
            },
            DatabaseOptions,
            cancellationToken);
    }

    public async Task<T?> ExecuteTransactionAsync<T>(
        Func<FamilyDbContext, Task<T>> operation,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        return await _circuitBreakerService.ExecuteAsync(
            $"database-transaction-{operationName}",
            async () =>
            {
                await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
                using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
                
                try
                {
                    var result = await operation(context);
                    await transaction.CommitAsync(cancellationToken);
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            },
            DatabaseOptions,
            cancellationToken);
    }
}

// Application/Services/FamilyQueryService.cs
public class FamilyQueryService : IFamilyQueryService
{
    private readonly ICircuitBreakerDbService _dbService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<FamilyQueryService> _logger;

    public FamilyQueryService(
        ICircuitBreakerDbService dbService,
        IMemoryCache cache,
        ILogger<FamilyQueryService> logger)
    {
        _dbService = dbService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<FamilyDto?> GetFamilyByIdAsync(
        Guid familyId, 
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"family_{familyId}";
        
        if (_cache.TryGetValue(cacheKey, out FamilyDto cachedFamily))
        {
            return cachedFamily;
        }

        try
        {
            var family = await _dbService.ExecuteQueryAsync(
                async context => await context.Families
                    .Include(f => f.Members)
                    .FirstOrDefaultAsync(f => f.Id == familyId, cancellationToken),
                "get-family-by-id",
                cancellationToken);

            if (family != null)
            {
                var familyDto = MapToDto(family);
                _cache.Set(cacheKey, familyDto, TimeSpan.FromMinutes(5));
                return familyDto;
            }

            return null;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogWarning("Database circuit breaker is open, returning cached data: {Message}", ex.Message);
            
            // Try to return stale cached data if available
            if (_cache.TryGetValue($"{cacheKey}_stale", out FamilyDto staleFamily))
            {
                return staleFamily;
            }

            throw new ServiceUnavailableException("Family service is temporarily unavailable", ex);
        }
    }

    private static FamilyDto MapToDto(Family family)
    {
        return new FamilyDto(
            family.Id,
            family.Name,
            family.Members.Select(m => new FamilyMemberDto(
                m.Id,
                m.FirstName,
                m.LastName,
                m.Email,
                m.DateOfBirth,
                family.Id
            )).ToList()
        );
    }
}
```

## 4. Configuration

### appsettings.json Configuration
```json
{
  "CircuitBreaker": {
    "school-api-students": {
      "FailureThreshold": 3,
      "DurationOfBreak": "00:02:00",
      "SuccessThreshold": 2,
      "Timeout": "00:00:15"
    },
    "school-api-grades": {
      "FailureThreshold": 5,
      "DurationOfBreak": "00:01:00",
      "SuccessThreshold": 3,
      "Timeout": "00:00:10"
    },
    "database-get-family-by-id": {
      "FailureThreshold": 5,
      "DurationOfBreak": "00:01:00",
      "SuccessThreshold": 3,
      "Timeout": "00:00:30"
    },
    "external-calendar-api": {
      "FailureThreshold": 2,
      "DurationOfBreak": "00:05:00",
      "SuccessThreshold": 1,
      "Timeout": "00:00:20"
    }
  },
  "SchoolApi": {
    "BaseUrl": "https://api.school.example.com",
    "ApiKey": "your-api-key",
    "Timeout": "00:00:30"
  }
}
```

### Service Registration
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Circuit Breaker Services
builder.Services.AddSingleton<ICircuitBreakerService, CircuitBreakerService>();
builder.Services.AddScoped<IEnhancedCircuitBreakerService, EnhancedCircuitBreakerService>();

// HTTP Clients with Circuit Breaker
builder.Services.AddHttpClient<ICircuitBreakerHttpService, CircuitBreakerHttpService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "Family-Platform/1.0");
});

// Database Services with Circuit Breaker
builder.Services.AddScoped<ICircuitBreakerDbService, CircuitBreakerDbService>();

// External API Services
builder.Services.Configure<SchoolApiOptions>(
    builder.Configuration.GetSection("SchoolApi"));
builder.Services.AddScoped<ISchoolApiService, SchoolApiService>();

// Query Services
builder.Services.AddScoped<IFamilyQueryService, FamilyQueryService>();

// Memory Cache for fallbacks
builder.Services.AddMemoryCache();

var app = builder.Build();
```

## 5. Health Checks Integration

### Circuit Breaker Health Check
```csharp
// Infrastructure/HealthChecks/CircuitBreakerHealthCheck.cs
public class CircuitBreakerHealthCheck : IHealthCheck
{
    private readonly ICircuitBreakerService _circuitBreakerService;
    private readonly ILogger<CircuitBreakerHealthCheck> _logger;

    public CircuitBreakerHealthCheck(
        ICircuitBreakerService circuitBreakerService,
        ILogger<CircuitBreakerHealthCheck> logger)
    {
        _circuitBreakerService = circuitBreakerService;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var unhealthyResults = new List<string>();
        var degradedResults = new List<string>();

        // Check critical circuit breakers
        var criticalCircuitBreakers = new[]
        {
            "database-get-family-by-id",
            "database-transaction-save-family"
        };

        foreach (var circuitBreakerName in criticalCircuitBreakers)
        {
            var policy = _circuitBreakerService.GetCircuitBreaker(circuitBreakerName);
            var state = GetCircuitBreakerState(policy);

            switch (state)
            {
                case CircuitBreakerState.Open:
                    unhealthyResults.Add($"Circuit breaker '{circuitBreakerName}' is OPEN");
                    break;
                case CircuitBreakerState.HalfOpen:
                    degradedResults.Add($"Circuit breaker '{circuitBreakerName}' is HALF-OPEN");
                    break;
            }
        }

        if (unhealthyResults.Any())
        {
            var message = string.Join("; ", unhealthyResults);
            _logger.LogWarning("Health check failed: {Message}", message);
            return HealthCheckResult.Unhealthy(message);
        }

        if (degradedResults.Any())
        {
            var message = string.Join("; ", degradedResults);
            _logger.LogInformation("Health check degraded: {Message}", message);
            return HealthCheckResult.Degraded(message);
        }

        return HealthCheckResult.Healthy("All circuit breakers are closed");
    }

    private static CircuitBreakerState GetCircuitBreakerState(IAsyncPolicy policy)
    {
        // This is a simplified example - in reality, you'd need to access Polly's internal state
        // or implement your own state tracking
        try
        {
            // Attempt a quick, non-impactful operation to test the circuit breaker state
            policy.ExecuteAsync(() => Task.CompletedTask).Wait(100);
            return CircuitBreakerState.Closed;
        }
        catch (BrokenCircuitException)
        {
            return CircuitBreakerState.Open;
        }
        catch
        {
            return CircuitBreakerState.HalfOpen;
        }
    }
}

public enum CircuitBreakerState
{
    Closed,
    Open,
    HalfOpen
}

// Register Health Check
builder.Services.AddHealthChecks()
    .AddCheck<CircuitBreakerHealthCheck>("circuit-breaker");
```

## 6. Testing Circuit Breaker

### Unit Tests
```csharp
// Tests/Unit/CircuitBreakerServiceTests.cs
public class CircuitBreakerServiceTests
{
    private readonly CircuitBreakerService _service;
    private readonly ILogger<CircuitBreakerService> _logger;
    private readonly IConfiguration _configuration;

    public CircuitBreakerServiceTests()
    {
        _logger = Substitute.For<ILogger<CircuitBreakerService>>();
        _configuration = Substitute.For<IConfiguration>();
        _service = new CircuitBreakerService(_logger, _configuration);
    }

    [Fact]
    public async Task ExecuteAsync_SuccessfulOperation_ReturnsResult()
    {
        // Arrange
        const string expectedResult = "success";
        var operation = () => Task.FromResult(expectedResult);

        // Act
        var result = await _service.ExecuteAsync("test-circuit", operation);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task ExecuteAsync_FailuresExceedThreshold_OpenCircuit()
    {
        // Arrange
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 2,
            DurationOfBreak = TimeSpan.FromSeconds(1)
        };

        var failingOperation = () => throw new HttpRequestException("Service unavailable");

        // Act & Assert - First two calls should fail normally
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            _service.ExecuteAsync("test-circuit", failingOperation, options));
        
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            _service.ExecuteAsync("test-circuit", failingOperation, options));

        // Third call should throw BrokenCircuitException (circuit is now open)
        await Assert.ThrowsAsync<BrokenCircuitException>(() => 
            _service.ExecuteAsync("test-circuit", failingOperation, options));
    }

    [Fact]
    public async Task ExecuteAsync_CircuitOpens_ThenResetsAfterDuration()
    {
        // Arrange
        var options = new CircuitBreakerOptions
        {
            FailureThreshold = 1,
            DurationOfBreak = TimeSpan.FromMilliseconds(100)
        };

        var callCount = 0;
        var operation = () =>
        {
            callCount++;
            if (callCount <= 1)
                throw new HttpRequestException("First call fails");
            return Task.FromResult("success");
        };

        // Act & Assert
        // First call fails, opens circuit
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            _service.ExecuteAsync("test-circuit", operation, options));

        // Immediate retry fails due to open circuit
        await Assert.ThrowsAsync<BrokenCircuitException>(() => 
            _service.ExecuteAsync("test-circuit", operation, options));

        // Wait for circuit to enter half-open state
        await Task.Delay(150);

        // Next call should succeed and close the circuit
        var result = await _service.ExecuteAsync("test-circuit", operation, options);
        result.Should().Be("success");
    }
}
```

### Integration Tests
```csharp
// Tests/Integration/SchoolApiServiceIntegrationTests.cs
public class SchoolApiServiceIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SchoolApiServiceIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetStudentsAsync_ServiceDown_CircuitBreakerOpensAndReturnsCachedData()
    {
        // Arrange
        var schoolApiService = _factory.Services.GetRequiredService<ISchoolApiService>();
        var schoolId = "test-school-123";

        // Simulate service being down by configuring mock to return failures
        _factory.MockSchoolApiToReturnFailures();

        // Act & Assert
        // First few calls should fail normally
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            schoolApiService.GetStudentsAsync(schoolId));
        
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            schoolApiService.GetStudentsAsync(schoolId));
        
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            schoolApiService.GetStudentsAsync(schoolId));

        // Circuit should now be open, next call should return cached data
        var cachedStudents = await schoolApiService.GetStudentsAsync(schoolId);
        cachedStudents.Should().BeNull(); // Or return actual cached data if implemented
    }

    [Fact]
    public async Task GetStudentsAsync_ServiceRecovers_CircuitBreakerCloses()
    {
        // Arrange
        var schoolApiService = _factory.Services.GetRequiredService<ISchoolApiService>();
        var schoolId = "test-school-123";

        // First, open the circuit breaker
        _factory.MockSchoolApiToReturnFailures();
        
        for (int i = 0; i < 3; i++)
        {
            try
            {
                await schoolApiService.GetStudentsAsync(schoolId);
            }
            catch
            {
                // Ignore failures, we're just opening the circuit
            }
        }

        // Now configure service to work again
        _factory.MockSchoolApiToReturnSuccess();

        // Wait for circuit to enter half-open state
        await Task.Delay(TimeSpan.FromSeconds(2));

        // Act
        var students = await schoolApiService.GetStudentsAsync(schoolId);

        // Assert
        students.Should().NotBeNull();
        students.Should().NotBeEmpty();
    }
}
```

## 7. Monitoring und Metrics

### Circuit Breaker Metrics
```csharp
// Infrastructure/Metrics/CircuitBreakerMetrics.cs
public class CircuitBreakerMetrics : ICircuitBreakerMetrics
{
    private readonly IMetricsLogger _metricsLogger;
    private readonly Counter<int> _circuitBreakerStateChanges;
    private readonly Counter<int> _circuitBreakerExecutions;
    private readonly Histogram<double> _executionDuration;

    public CircuitBreakerMetrics(IMeterFactory meterFactory, IMetricsLogger metricsLogger)
    {
        _metricsLogger = metricsLogger;
        var meter = meterFactory.Create("Family.CircuitBreaker");
        
        _circuitBreakerStateChanges = meter.CreateCounter<int>(
            "circuit_breaker_state_changes",
            "Number of circuit breaker state changes");
            
        _circuitBreakerExecutions = meter.CreateCounter<int>(
            "circuit_breaker_executions",
            "Number of circuit breaker executions");
            
        _executionDuration = meter.CreateHistogram<double>(
            "circuit_breaker_execution_duration",
            "ms", "Duration of circuit breaker executions");
    }

    public void RecordStateChange(string circuitBreakerName, CircuitBreakerState newState)
    {
        _circuitBreakerStateChanges.Add(1, 
            new KeyValuePair<string, object?>("circuit_breaker", circuitBreakerName),
            new KeyValuePair<string, object?>("state", newState.ToString()));

        _metricsLogger.LogInformation(
            "Circuit breaker {CircuitBreakerName} changed state to {State}",
            circuitBreakerName, newState);
    }

    public void RecordExecution(
        string circuitBreakerName, 
        bool success, 
        TimeSpan duration, 
        string? errorType = null)
    {
        _circuitBreakerExecutions.Add(1,
            new KeyValuePair<string, object?>("circuit_breaker", circuitBreakerName),
            new KeyValuePair<string, object?>("result", success ? "success" : "failure"),
            new KeyValuePair<string, object?>("error_type", errorType ?? "none"));

        _executionDuration.Record(duration.TotalMilliseconds,
            new KeyValuePair<string, object?>("circuit_breaker", circuitBreakerName),
            new KeyValuePair<string, object?>("result", success ? "success" : "failure"));

        if (!success)
        {
            _metricsLogger.LogWarning(
                "Circuit breaker {CircuitBreakerName} execution failed: {ErrorType} (Duration: {Duration}ms)",
                circuitBreakerName, errorType, duration.TotalMilliseconds);
        }
    }
}
```

## Benefits des Circuit Breaker Patterns

1. **Failure Isolation**: Verhindert Cascade-Failures zwischen Services
2. **Fast Failure**: Sofortige Fehlerbehandlung statt lange Timeouts
3. **Automatic Recovery**: Automatisches Testen der Service-Verfügbarkeit
4. **Resource Protection**: Schutz vor Überlastung downstream Services
5. **Graceful Degradation**: Fallback-Mechanismen für bessere User Experience
6. **Monitoring**: Detaillierte Metrics für Service-Health
7. **Configuration**: Flexible Konfiguration per Service/Operation