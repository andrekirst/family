# Monitoring & Observability für Cloud Design Patterns

## Übersicht

Diese Dokumentation beschreibt umfassende Monitoring- und Observability-Strategien für die implementierten Cloud Design Patterns. Ein effektives Monitoring-System ermöglicht es, die Performance, Gesundheit und das Verhalten der Pattern-Implementierungen zu überwachen und Probleme proaktiv zu identifizieren.

## 1. OpenTelemetry Integration

### Base Configuration
```csharp
// Program.cs
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;

var builder = WebApplication.CreateBuilder(args);

// OpenTelemetry Configuration
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("Family.API", "1.0.0")
        .AddAttributes(new Dictionary<string, object>
        {
            ["deployment.environment"] = builder.Environment.EnvironmentName,
            ["service.instance.id"] = Environment.MachineName
        }))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            options.EnrichWithHttpRequest = EnrichHttpRequest;
            options.EnrichWithHttpResponse = EnrichHttpResponse;
        })
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddRedisInstrumentation()
        .AddSource("Family.CQRS")
        .AddSource("Family.EventSourcing")
        .AddSource("Family.CircuitBreaker")
        .AddJaegerExporter()
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddMeter("Family.CQRS")
        .AddMeter("Family.EventSourcing")
        .AddMeter("Family.CircuitBreaker")
        .AddPrometheusExporter()
        .AddConsoleExporter());

// Add custom metrics
builder.Services.AddSingleton<IMetrics, CustomMetrics>();

static void EnrichHttpRequest(Activity activity, HttpRequest request)
{
    activity.SetTag("http.user_agent", request.Headers.UserAgent.ToString());
    activity.SetTag("http.client_ip", request.HttpContext.Connection.RemoteIpAddress?.ToString());
}

static void EnrichHttpResponse(Activity activity, HttpResponse response)
{
    activity.SetTag("http.response.status_code", response.StatusCode);
}
```

### Custom Metrics Service
```csharp
// Infrastructure/Metrics/CustomMetrics.cs
public class CustomMetrics : IMetrics
{
    private readonly Meter _meter;
    private readonly ILogger<CustomMetrics> _logger;

    // CQRS Metrics
    private readonly Counter<int> _commandsProcessed;
    private readonly Counter<int> _queriesProcessed;
    private readonly Histogram<double> _commandProcessingTime;
    private readonly Histogram<double> _queryProcessingTime;

    // Event Sourcing Metrics
    private readonly Counter<int> _eventsStored;
    private readonly Counter<int> _aggregatesLoaded;
    private readonly Histogram<double> _eventStoreOperationTime;
    private readonly Histogram<int> _eventCountPerAggregate;

    // Circuit Breaker Metrics
    private readonly Counter<int> _circuitBreakerStateChanges;
    private readonly Counter<int> _circuitBreakerExecutions;
    private readonly Gauge<int> _openCircuitBreakers;

    // General Performance Metrics
    private readonly Histogram<double> _databaseOperationTime;
    private readonly Counter<int> _cacheOperations;
    private readonly Gauge<double> _cacheHitRatio;

    public CustomMetrics(IMeterFactory meterFactory, ILogger<CustomMetrics> logger)
    {
        _logger = logger;
        _meter = meterFactory.Create("Family.Platform");

        // Initialize CQRS metrics
        _commandsProcessed = _meter.CreateCounter<int>(
            "family_commands_total", 
            "Total number of commands processed");
        
        _queriesProcessed = _meter.CreateCounter<int>(
            "family_queries_total", 
            "Total number of queries processed");
        
        _commandProcessingTime = _meter.CreateHistogram<double>(
            "family_command_duration_ms", 
            "Command processing time in milliseconds");
        
        _queryProcessingTime = _meter.CreateHistogram<double>(
            "family_query_duration_ms", 
            "Query processing time in milliseconds");

        // Initialize Event Sourcing metrics
        _eventsStored = _meter.CreateCounter<int>(
            "family_events_stored_total", 
            "Total number of events stored");
        
        _aggregatesLoaded = _meter.CreateCounter<int>(
            "family_aggregates_loaded_total", 
            "Total number of aggregates loaded from event store");
        
        _eventStoreOperationTime = _meter.CreateHistogram<double>(
            "family_event_store_operation_duration_ms", 
            "Event store operation time in milliseconds");
        
        _eventCountPerAggregate = _meter.CreateHistogram<int>(
            "family_events_per_aggregate", 
            "Number of events per aggregate");

        // Initialize Circuit Breaker metrics
        _circuitBreakerStateChanges = _meter.CreateCounter<int>(
            "family_circuit_breaker_state_changes_total", 
            "Total number of circuit breaker state changes");
        
        _circuitBreakerExecutions = _meter.CreateCounter<int>(
            "family_circuit_breaker_executions_total", 
            "Total number of circuit breaker executions");
        
        _openCircuitBreakers = _meter.CreateGauge<int>(
            "family_circuit_breakers_open", 
            "Number of currently open circuit breakers");

        // Initialize general metrics
        _databaseOperationTime = _meter.CreateHistogram<double>(
            "family_database_operation_duration_ms", 
            "Database operation time in milliseconds");
        
        _cacheOperations = _meter.CreateCounter<int>(
            "family_cache_operations_total", 
            "Total number of cache operations");
        
        _cacheHitRatio = _meter.CreateGauge<double>(
            "family_cache_hit_ratio", 
            "Cache hit ratio");
    }

    // CQRS Metrics Methods
    public void RecordCommandProcessed(string commandType, double durationMs, bool success)
    {
        _commandsProcessed.Add(1, 
            new KeyValuePair<string, object?>("command_type", commandType),
            new KeyValuePair<string, object?>("result", success ? "success" : "failure"));
        
        _commandProcessingTime.Record(durationMs,
            new KeyValuePair<string, object?>("command_type", commandType),
            new KeyValuePair<string, object?>("result", success ? "success" : "failure"));
    }

    public void RecordQueryProcessed(string queryType, double durationMs, bool success, int resultCount = 0)
    {
        _queriesProcessed.Add(1,
            new KeyValuePair<string, object?>("query_type", queryType),
            new KeyValuePair<string, object?>("result", success ? "success" : "failure"));
        
        _queryProcessingTime.Record(durationMs,
            new KeyValuePair<string, object?>("query_type", queryType),
            new KeyValuePair<string, object?>("result", success ? "success" : "failure"),
            new KeyValuePair<string, object?>("result_count", resultCount));
    }

    // Event Sourcing Metrics Methods
    public void RecordEventsStored(string aggregateType, int eventCount, double durationMs)
    {
        _eventsStored.Add(eventCount,
            new KeyValuePair<string, object?>("aggregate_type", aggregateType));
        
        _eventStoreOperationTime.Record(durationMs,
            new KeyValuePair<string, object?>("operation", "store_events"),
            new KeyValuePair<string, object?>("aggregate_type", aggregateType));
    }

    public void RecordAggregateLoaded(string aggregateType, int eventCount, double durationMs)
    {
        _aggregatesLoaded.Add(1,
            new KeyValuePair<string, object?>("aggregate_type", aggregateType));
        
        _eventCountPerAggregate.Record(eventCount,
            new KeyValuePair<string, object?>("aggregate_type", aggregateType));
        
        _eventStoreOperationTime.Record(durationMs,
            new KeyValuePair<string, object?>("operation", "load_aggregate"),
            new KeyValuePair<string, object?>("aggregate_type", aggregateType));
    }

    // Circuit Breaker Metrics Methods
    public void RecordCircuitBreakerStateChange(string circuitBreakerName, string fromState, string toState)
    {
        _circuitBreakerStateChanges.Add(1,
            new KeyValuePair<string, object?>("circuit_breaker", circuitBreakerName),
            new KeyValuePair<string, object?>("from_state", fromState),
            new KeyValuePair<string, object?>("to_state", toState));
    }

    public void RecordCircuitBreakerExecution(string circuitBreakerName, bool success, double durationMs, string? errorType = null)
    {
        _circuitBreakerExecutions.Add(1,
            new KeyValuePair<string, object?>("circuit_breaker", circuitBreakerName),
            new KeyValuePair<string, object?>("result", success ? "success" : "failure"),
            new KeyValuePair<string, object?>("error_type", errorType ?? "none"));
    }

    public void SetOpenCircuitBreakers(int count)
    {
        _openCircuitBreakers.Record(count);
    }

    // General Metrics Methods
    public void RecordDatabaseOperation(string operation, double durationMs, bool success)
    {
        _databaseOperationTime.Record(durationMs,
            new KeyValuePair<string, object?>("operation", operation),
            new KeyValuePair<string, object?>("result", success ? "success" : "failure"));
    }

    public void RecordCacheOperation(string operation, bool hit)
    {
        _cacheOperations.Add(1,
            new KeyValuePair<string, object?>("operation", operation),
            new KeyValuePair<string, object?>("result", hit ? "hit" : "miss"));
    }

    public void SetCacheHitRatio(double ratio)
    {
        _cacheHitRatio.Record(ratio);
    }
}
```

## 2. Distributed Tracing

### CQRS Tracing
```csharp
// Application/Behaviors/TracingBehavior.cs
public class TracingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly ActivitySource ActivitySource = new("Family.CQRS");
    private readonly IMetrics _metrics;
    private readonly ILogger<TracingBehavior<TRequest, TResponse>> _logger;

    public TracingBehavior(IMetrics metrics, ILogger<TracingBehavior<TRequest, TResponse>> logger)
    {
        _metrics = metrics;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;
        var isCommand = typeof(TRequest).Name.EndsWith("Command");
        var operationType = isCommand ? "command" : "query";

        using var activity = ActivitySource.StartActivity($"{operationType}: {requestType}");
        activity?.SetTag("cqrs.operation_type", operationType);
        activity?.SetTag("cqrs.request_type", requestType);
        activity?.SetTag("cqrs.correlation_id", Guid.NewGuid().ToString());

        var stopwatch = Stopwatch.StartNew();
        var success = false;
        
        try
        {
            _logger.LogDebug("Processing {OperationType} {RequestType}", operationType, requestType);
            
            var response = await next();
            success = true;
            
            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag("cqrs.result", "success");
            
            return response;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("cqrs.result", "failure");
            activity?.SetTag("cqrs.error_type", ex.GetType().Name);
            activity?.SetTag("cqrs.error_message", ex.Message);
            
            _logger.LogError(ex, "Error processing {OperationType} {RequestType}", operationType, requestType);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            var durationMs = stopwatch.Elapsed.TotalMilliseconds;
            
            activity?.SetTag("cqrs.duration_ms", durationMs);
            
            if (isCommand)
            {
                _metrics.RecordCommandProcessed(requestType, durationMs, success);
            }
            else
            {
                var resultCount = ExtractResultCount(typeof(TResponse));
                _metrics.RecordQueryProcessed(requestType, durationMs, success, resultCount);
            }
            
            _logger.LogDebug("Completed {OperationType} {RequestType} in {Duration}ms", 
                operationType, requestType, durationMs);
        }
    }

    private static int ExtractResultCount(Type responseType)
    {
        // Try to extract count from common response patterns
        if (responseType.IsGenericType)
        {
            var genericType = responseType.GetGenericTypeDefinition();
            if (genericType == typeof(IEnumerable<>) || genericType == typeof(List<>))
            {
                return 1; // Simplified - in practice, you'd extract actual count
            }
        }
        
        return 0;
    }
}
```

### Event Sourcing Tracing
```csharp
// Infrastructure/EventStore/TracingEventStore.cs
public class TracingEventStore : IEventStore
{
    private static readonly ActivitySource ActivitySource = new("Family.EventSourcing");
    private readonly IEventStore _innerEventStore;
    private readonly IMetrics _metrics;
    private readonly ILogger<TracingEventStore> _logger;

    public TracingEventStore(IEventStore innerEventStore, IMetrics metrics, ILogger<TracingEventStore> logger)
    {
        _innerEventStore = innerEventStore;
        _metrics = metrics;
        _logger = logger;
    }

    public async Task SaveEventsAsync<T>(
        Guid aggregateId, 
        IEnumerable<DomainEvent> events, 
        int expectedVersion,
        CancellationToken cancellationToken = default) where T : AggregateRoot
    {
        var eventList = events.ToList();
        var aggregateType = typeof(T).Name;
        
        using var activity = ActivitySource.StartActivity("EventStore.SaveEvents");
        activity?.SetTag("event_sourcing.operation", "save_events");
        activity?.SetTag("event_sourcing.aggregate_type", aggregateType);
        activity?.SetTag("event_sourcing.aggregate_id", aggregateId.ToString());
        activity?.SetTag("event_sourcing.event_count", eventList.Count);
        activity?.SetTag("event_sourcing.expected_version", expectedVersion);

        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogDebug("Saving {EventCount} events for aggregate {AggregateId} (type: {AggregateType})",
                eventList.Count, aggregateId, aggregateType);

            await _innerEventStore.SaveEventsAsync<T>(aggregateId, eventList, expectedVersion, cancellationToken);
            
            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.AddEvent(new ActivityEvent("Events saved successfully"));
            
            _logger.LogInformation("Successfully saved {EventCount} events for aggregate {AggregateId}",
                eventList.Count, aggregateId);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("event_sourcing.error_type", ex.GetType().Name);
            
            _logger.LogError(ex, "Failed to save events for aggregate {AggregateId}", aggregateId);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            var durationMs = stopwatch.Elapsed.TotalMilliseconds;
            
            activity?.SetTag("event_sourcing.duration_ms", durationMs);
            _metrics.RecordEventsStored(aggregateType, eventList.Count, durationMs);
        }
    }

    public async Task<IEnumerable<DomainEvent>> GetEventsAsync(
        Guid aggregateId,
        int fromVersion = 0,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("EventStore.GetEvents");
        activity?.SetTag("event_sourcing.operation", "get_events");
        activity?.SetTag("event_sourcing.aggregate_id", aggregateId.ToString());
        activity?.SetTag("event_sourcing.from_version", fromVersion);

        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var events = await _innerEventStore.GetEventsAsync(aggregateId, fromVersion, cancellationToken);
            var eventList = events.ToList();
            
            activity?.SetTag("event_sourcing.event_count", eventList.Count);
            activity?.SetStatus(ActivityStatusCode.Ok);
            
            return eventList;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("event_sourcing.error_type", ex.GetType().Name);
            
            _logger.LogError(ex, "Failed to get events for aggregate {AggregateId}", aggregateId);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            activity?.SetTag("event_sourcing.duration_ms", stopwatch.Elapsed.TotalMilliseconds);
        }
    }

    public async Task<T?> GetAggregateAsync<T>(
        Guid aggregateId,
        CancellationToken cancellationToken = default) where T : AggregateRoot, new()
    {
        var aggregateType = typeof(T).Name;
        
        using var activity = ActivitySource.StartActivity("EventStore.GetAggregate");
        activity?.SetTag("event_sourcing.operation", "get_aggregate");
        activity?.SetTag("event_sourcing.aggregate_type", aggregateType);
        activity?.SetTag("event_sourcing.aggregate_id", aggregateId.ToString());

        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var aggregate = await _innerEventStore.GetAggregateAsync<T>(aggregateId, cancellationToken);
            
            if (aggregate != null)
            {
                activity?.SetTag("event_sourcing.aggregate_version", aggregate.Version);
                activity?.SetTag("event_sourcing.found", true);
                
                _metrics.RecordAggregateLoaded(aggregateType, aggregate.Version, stopwatch.Elapsed.TotalMilliseconds);
            }
            else
            {
                activity?.SetTag("event_sourcing.found", false);
            }
            
            activity?.SetStatus(ActivityStatusCode.Ok);
            return aggregate;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("event_sourcing.error_type", ex.GetType().Name);
            
            _logger.LogError(ex, "Failed to get aggregate {AggregateId}", aggregateId);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            activity?.SetTag("event_sourcing.duration_ms", stopwatch.Elapsed.TotalMilliseconds);
        }
    }
}
```

## 3. Health Checks

### Pattern-Specific Health Checks
```csharp
// Infrastructure/HealthChecks/PatternHealthChecks.cs
public class CQRSHealthCheck : IHealthCheck
{
    private readonly IMediator _mediator;
    private readonly ILogger<CQRSHealthCheck> _logger;

    public CQRSHealthCheck(IMediator mediator, ILogger<CQRSHealthCheck> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Test with a simple health check query
            var healthQuery = new HealthCheckQuery();
            var result = await _mediator.Send(healthQuery, cancellationToken);
            
            if (result.IsHealthy)
            {
                return HealthCheckResult.Healthy("CQRS pipeline is functioning correctly");
            }
            
            return HealthCheckResult.Degraded("CQRS pipeline is experiencing issues");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CQRS health check failed");
            return HealthCheckResult.Unhealthy("CQRS pipeline is not responding", ex);
        }
    }
}

public class EventStoreHealthCheck : IHealthCheck
{
    private readonly IEventStore _eventStore;
    private readonly ILogger<EventStoreHealthCheck> _logger;

    public EventStoreHealthCheck(IEventStore eventStore, ILogger<EventStoreHealthCheck> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Test event store connectivity with a dummy aggregate ID
            var testAggregateId = Guid.NewGuid();
            var events = await _eventStore.GetEventsAsync(testAggregateId, 0, cancellationToken);
            
            // If we can query (even with no results), the event store is healthy
            return HealthCheckResult.Healthy("Event store is accessible");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Event store health check failed");
            return HealthCheckResult.Unhealthy("Event store is not accessible", ex);
        }
    }
}

public class CircuitBreakerHealthCheck : IHealthCheck
{
    private readonly ICircuitBreakerService _circuitBreakerService;
    private readonly ILogger<CircuitBreakerHealthCheck> _logger;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var criticalCircuitBreakers = new[]
        {
            "database-operations",
            "external-school-api",
            "event-publishing"
        };

        var degradedCircuitBreakers = new List<string>();
        var failedCircuitBreakers = new List<string>();

        foreach (var circuitBreakerName in criticalCircuitBreakers)
        {
            try
            {
                // Test circuit breaker state
                var isHealthy = await TestCircuitBreakerAsync(circuitBreakerName, cancellationToken);
                
                if (!isHealthy)
                {
                    degradedCircuitBreakers.Add(circuitBreakerName);
                }
            }
            catch (BrokenCircuitException)
            {
                failedCircuitBreakers.Add(circuitBreakerName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check circuit breaker {CircuitBreakerName}", circuitBreakerName);
                failedCircuitBreakers.Add(circuitBreakerName);
            }
        }

        if (failedCircuitBreakers.Any())
        {
            var message = $"Circuit breakers are open: {string.Join(", ", failedCircuitBreakers)}";
            return HealthCheckResult.Unhealthy(message);
        }

        if (degradedCircuitBreakers.Any())
        {
            var message = $"Circuit breakers are degraded: {string.Join(", ", degradedCircuitBreakers)}";
            return HealthCheckResult.Degraded(message);
        }

        return HealthCheckResult.Healthy("All circuit breakers are healthy");
    }

    private async Task<bool> TestCircuitBreakerAsync(string circuitBreakerName, CancellationToken cancellationToken)
    {
        // Implement specific test for each circuit breaker
        return await _circuitBreakerService.ExecuteAsync(
            circuitBreakerName,
            () => Task.FromResult(true),
            new CircuitBreakerOptions { Timeout = TimeSpan.FromSeconds(5) },
            cancellationToken);
    }
}
```

### Health Check Configuration
```csharp
// Program.cs - Health Checks Configuration
builder.Services.AddHealthChecks()
    .AddCheck<CQRSHealthCheck>("cqrs", tags: ["pattern", "cqrs"])
    .AddCheck<EventStoreHealthCheck>("event-store", tags: ["pattern", "event-sourcing"])
    .AddCheck<CircuitBreakerHealthCheck>("circuit-breaker", tags: ["pattern", "circuit-breaker"])
    .AddNpgSql(connectionString, tags: ["database"])
    .AddRedis(redisConnectionString, tags: ["cache"])
    .AddKafka(options =>
    {
        options.BootstrapServers = kafkaBootstrapServers;
    }, tags: ["messaging"]);

var app = builder.Build();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/patterns", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("pattern")
});
```

## 4. Logging

### Structured Logging für Patterns
```csharp
// Infrastructure/Logging/PatternLoggerExtensions.cs
public static class PatternLoggerExtensions
{
    // CQRS Logging
    public static void LogCommandStarted<T>(this ILogger logger, T command, string correlationId)
    {
        logger.LogInformation("Command {CommandType} started with correlation ID {CorrelationId}. Data: {@Command}",
            typeof(T).Name, correlationId, command);
    }

    public static void LogCommandCompleted<T>(this ILogger logger, T command, string correlationId, TimeSpan duration)
    {
        logger.LogInformation("Command {CommandType} completed in {Duration}ms with correlation ID {CorrelationId}",
            typeof(T).Name, duration.TotalMilliseconds, correlationId);
    }

    public static void LogCommandFailed<T>(this ILogger logger, T command, string correlationId, Exception exception)
    {
        logger.LogError(exception, "Command {CommandType} failed with correlation ID {CorrelationId}. Data: {@Command}",
            typeof(T).Name, correlationId, command);
    }

    public static void LogQueryStarted<T>(this ILogger logger, T query, string correlationId)
    {
        logger.LogDebug("Query {QueryType} started with correlation ID {CorrelationId}. Parameters: {@Query}",
            typeof(T).Name, correlationId, query);
    }

    public static void LogQueryCompleted<T>(this ILogger logger, T query, string correlationId, TimeSpan duration, int resultCount)
    {
        logger.LogInformation("Query {QueryType} completed in {Duration}ms with {ResultCount} results and correlation ID {CorrelationId}",
            typeof(T).Name, duration.TotalMilliseconds, resultCount, correlationId);
    }

    // Event Sourcing Logging
    public static void LogEventStored(this ILogger logger, DomainEvent domainEvent, Guid aggregateId, int version)
    {
        logger.LogInformation("Event {EventType} stored for aggregate {AggregateId} at version {Version}. Data: {@Event}",
            domainEvent.GetType().Name, aggregateId, version, domainEvent);
    }

    public static void LogAggregateLoaded(this ILogger logger, string aggregateType, Guid aggregateId, int version, int eventCount)
    {
        logger.LogDebug("Aggregate {AggregateType} {AggregateId} loaded at version {Version} from {EventCount} events",
            aggregateType, aggregateId, version, eventCount);
    }

    public static void LogEventProjectionStarted(this ILogger logger, DomainEvent domainEvent, string projectionName)
    {
        logger.LogDebug("Projection {ProjectionName} started processing event {EventType} {EventId}",
            projectionName, domainEvent.GetType().Name, domainEvent.Id);
    }

    public static void LogEventProjectionCompleted(this ILogger logger, DomainEvent domainEvent, string projectionName, TimeSpan duration)
    {
        logger.LogDebug("Projection {ProjectionName} completed processing event {EventType} {EventId} in {Duration}ms",
            projectionName, domainEvent.GetType().Name, domainEvent.Id, duration.TotalMilliseconds);
    }

    // Circuit Breaker Logging
    public static void LogCircuitBreakerOpened(this ILogger logger, string circuitBreakerName, TimeSpan duration, Exception exception)
    {
        logger.LogWarning(exception, "Circuit breaker {CircuitBreakerName} opened for {Duration}ms due to repeated failures",
            circuitBreakerName, duration.TotalMilliseconds);
    }

    public static void LogCircuitBreakerClosed(this ILogger logger, string circuitBreakerName)
    {
        logger.LogInformation("Circuit breaker {CircuitBreakerName} closed - service is healthy again",
            circuitBreakerName);
    }

    public static void LogCircuitBreakerHalfOpen(this ILogger logger, string circuitBreakerName)
    {
        logger.LogInformation("Circuit breaker {CircuitBreakerName} is half-open - testing service availability",
            circuitBreakerName);
    }

    public static void LogCircuitBreakerExecution(this ILogger logger, string circuitBreakerName, bool success, TimeSpan duration, string? errorType = null)
    {
        if (success)
        {
            logger.LogDebug("Circuit breaker {CircuitBreakerName} execution succeeded in {Duration}ms",
                circuitBreakerName, duration.TotalMilliseconds);
        }
        else
        {
            logger.LogWarning("Circuit breaker {CircuitBreakerName} execution failed in {Duration}ms with error type {ErrorType}",
                circuitBreakerName, duration.TotalMilliseconds, errorType);
        }
    }
}
```

### Correlation ID Middleware
```csharp
// Infrastructure/Middleware/CorrelationIdMiddleware.cs
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        
        // Add to response headers
        context.Response.Headers.TryAdd(CorrelationIdHeader, correlationId);
        
        // Add to logging context
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["RequestPath"] = context.Request.Path,
            ["RequestMethod"] = context.Request.Method
        }))
        {
            await _next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault();
        
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        context.Items["CorrelationId"] = correlationId;
        return correlationId;
    }
}
```

## 5. Application Performance Monitoring (APM)

### Custom APM Integration
```csharp
// Infrastructure/APM/FamilyAPMService.cs
public class FamilyAPMService : IFamilyAPMService
{
    private readonly ILogger<FamilyAPMService> _logger;
    private readonly IMetrics _metrics;
    private readonly DiagnosticSource _diagnosticSource;

    public FamilyAPMService(ILogger<FamilyAPMService> logger, IMetrics metrics)
    {
        _logger = logger;
        _metrics = metrics;
        _diagnosticSource = new DiagnosticListener("Family.APM");
    }

    public async Task<T> TrackOperationAsync<T>(
        string operationName,
        Func<Task<T>> operation,
        Dictionary<string, object>? properties = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var operationId = Guid.NewGuid().ToString();
        
        using var activity = Activity.Current?.Source.StartActivity(operationName);
        activity?.SetTag("operation.id", operationId);
        
        if (properties != null)
        {
            foreach (var prop in properties)
            {
                activity?.SetTag(prop.Key, prop.Value?.ToString());
            }
        }

        try
        {
            _logger.LogDebug("Starting operation {OperationName} with ID {OperationId}", operationName, operationId);
            
            var result = await operation();
            
            stopwatch.Stop();
            
            activity?.SetStatus(ActivityStatusCode.Ok);
            RecordOperationSuccess(operationName, stopwatch.Elapsed, properties);
            
            _logger.LogDebug("Completed operation {OperationName} with ID {OperationId} in {Duration}ms", 
                operationName, operationId, stopwatch.ElapsedMilliseconds);
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            RecordOperationFailure(operationName, stopwatch.Elapsed, ex, properties);
            
            _logger.LogError(ex, "Failed operation {OperationName} with ID {OperationId} after {Duration}ms", 
                operationName, operationId, stopwatch.ElapsedMilliseconds);
            
            throw;
        }
    }

    private void RecordOperationSuccess(string operationName, TimeSpan duration, Dictionary<string, object>? properties)
    {
        var tags = new List<KeyValuePair<string, object?>>
        {
            new("operation", operationName),
            new("result", "success")
        };

        if (properties != null)
        {
            tags.AddRange(properties.Select(p => new KeyValuePair<string, object?>(p.Key, p.Value)));
        }

        _metrics.RecordDatabaseOperation(operationName, duration.TotalMilliseconds, true);
    }

    private void RecordOperationFailure(string operationName, TimeSpan duration, Exception exception, Dictionary<string, object>? properties)
    {
        var tags = new List<KeyValuePair<string, object?>>
        {
            new("operation", operationName),
            new("result", "failure"),
            new("error_type", exception.GetType().Name)
        };

        if (properties != null)
        {
            tags.AddRange(properties.Select(p => new KeyValuePair<string, object?>(p.Key, p.Value)));
        }

        _metrics.RecordDatabaseOperation(operationName, duration.TotalMilliseconds, false);
    }
}
```

## 6. Dashboards und Alerting

### Grafana Dashboard Configuration
```json
{
  "dashboard": {
    "title": "Family Platform - Design Patterns",
    "panels": [
      {
        "title": "CQRS Operations",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(family_commands_total[5m])",
            "legendFormat": "Commands/sec"
          },
          {
            "expr": "rate(family_queries_total[5m])",
            "legendFormat": "Queries/sec"
          }
        ]
      },
      {
        "title": "Command Processing Time",
        "type": "histogram",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, rate(family_command_duration_ms_bucket[5m]))",
            "legendFormat": "95th percentile"
          },
          {
            "expr": "histogram_quantile(0.50, rate(family_command_duration_ms_bucket[5m]))",
            "legendFormat": "50th percentile"
          }
        ]
      },
      {
        "title": "Event Store Operations",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(family_events_stored_total[5m])",
            "legendFormat": "Events stored/sec"
          },
          {
            "expr": "rate(family_aggregates_loaded_total[5m])",
            "legendFormat": "Aggregates loaded/sec"
          }
        ]
      },
      {
        "title": "Circuit Breaker Status",
        "type": "singlestat",
        "targets": [
          {
            "expr": "family_circuit_breakers_open",
            "legendFormat": "Open Circuit Breakers"
          }
        ],
        "thresholds": [
          {
            "value": 0,
            "color": "green"
          },
          {
            "value": 1,
            "color": "red"
          }
        ]
      },
      {
        "title": "Database Performance",
        "type": "graph",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, rate(family_database_operation_duration_ms_bucket[5m]))",
            "legendFormat": "95th percentile"
          }
        ]
      },
      {
        "title": "Cache Hit Ratio",
        "type": "gauge",
        "targets": [
          {
            "expr": "family_cache_hit_ratio",
            "legendFormat": "Hit Ratio"
          }
        ]
      }
    ]
  }
}
```

### Prometheus Alerting Rules
```yaml
groups:
  - name: family_platform_patterns
    rules:
      - alert: HighCommandFailureRate
        expr: rate(family_commands_total{result="failure"}[5m]) / rate(family_commands_total[5m]) > 0.1
        for: 2m
        labels:
          severity: warning
          pattern: cqrs
        annotations:
          summary: "High command failure rate detected"
          description: "Command failure rate is {{ $value | humanizePercentage }} over the last 5 minutes"

      - alert: SlowCommandProcessing
        expr: histogram_quantile(0.95, rate(family_command_duration_ms_bucket[5m])) > 5000
        for: 5m
        labels:
          severity: warning
          pattern: cqrs
        annotations:
          summary: "Slow command processing detected"
          description: "95th percentile command processing time is {{ $value }}ms"

      - alert: EventStoreUnavailable
        expr: up{job="family-api"} == 0 or rate(family_events_stored_total[5m]) == 0
        for: 1m
        labels:
          severity: critical
          pattern: event_sourcing
        annotations:
          summary: "Event store appears to be unavailable"
          description: "No events have been stored in the last 5 minutes"

      - alert: CircuitBreakerOpen
        expr: family_circuit_breakers_open > 0
        for: 0m
        labels:
          severity: warning
          pattern: circuit_breaker
        annotations:
          summary: "Circuit breaker(s) are open"
          description: "{{ $value }} circuit breaker(s) are currently open"

      - alert: HighDatabaseLatency
        expr: histogram_quantile(0.95, rate(family_database_operation_duration_ms_bucket[5m])) > 1000
        for: 3m
        labels:
          severity: warning
          pattern: database
        annotations:
          summary: "High database latency detected"
          description: "95th percentile database operation time is {{ $value }}ms"

      - alert: LowCacheHitRatio
        expr: family_cache_hit_ratio < 0.7
        for: 5m
        labels:
          severity: warning
          pattern: cache
        annotations:
          summary: "Low cache hit ratio"
          description: "Cache hit ratio is {{ $value | humanizePercentage }}"
```

## 7. Performance Monitoring

### Custom Performance Counters
```csharp
// Infrastructure/Performance/PerformanceCounters.cs
public class PerformanceCounters : IPerformanceCounters
{
    private readonly IMetrics _metrics;
    private readonly Timer _performanceTimer;
    private readonly ConcurrentDictionary<string, PerformanceData> _performanceData = new();

    public PerformanceCounters(IMetrics metrics)
    {
        _metrics = metrics;
        _performanceTimer = new Timer(CollectPerformanceData, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }

    public void RecordOperation(string operationName, TimeSpan duration, bool success)
    {
        _performanceData.AddOrUpdate(operationName, 
            new PerformanceData(operationName),
            (key, existing) =>
            {
                existing.RecordOperation(duration, success);
                return existing;
            });
    }

    private void CollectPerformanceData(object? state)
    {
        foreach (var kvp in _performanceData)
        {
            var data = kvp.Value;
            var snapshot = data.GetSnapshot();
            
            _metrics.RecordPerformanceSnapshot(
                data.OperationName,
                snapshot.AverageResponseTime,
                snapshot.ThroughputPerSecond,
                snapshot.ErrorRate,
                snapshot.P95ResponseTime);
        }
    }

    public void Dispose()
    {
        _performanceTimer?.Dispose();
    }
}

public class PerformanceData
{
    private readonly string _operationName;
    private readonly List<double> _responseTimes = new();
    private readonly List<bool> _results = new();
    private readonly object _lock = new();
    private DateTime _lastSnapshot = DateTime.UtcNow;

    public PerformanceData(string operationName)
    {
        _operationName = operationName;
    }

    public string OperationName => _operationName;

    public void RecordOperation(TimeSpan duration, bool success)
    {
        lock (_lock)
        {
            _responseTimes.Add(duration.TotalMilliseconds);
            _results.Add(success);
        }
    }

    public PerformanceSnapshot GetSnapshot()
    {
        lock (_lock)
        {
            if (!_responseTimes.Any())
            {
                return new PerformanceSnapshot(_operationName, 0, 0, 0, 0);
            }

            var now = DateTime.UtcNow;
            var timePeriod = now - _lastSnapshot;
            
            var avgResponseTime = _responseTimes.Average();
            var throughput = _responseTimes.Count / timePeriod.TotalSeconds;
            var errorRate = _results.Count(r => !r) / (double)_results.Count;
            var p95ResponseTime = CalculatePercentile(_responseTimes, 0.95);

            // Clear data for next snapshot
            _responseTimes.Clear();
            _results.Clear();
            _lastSnapshot = now;

            return new PerformanceSnapshot(_operationName, avgResponseTime, throughput, errorRate, p95ResponseTime);
        }
    }

    private static double CalculatePercentile(List<double> values, double percentile)
    {
        if (!values.Any()) return 0;
        
        var sorted = values.OrderBy(x => x).ToList();
        var index = (int)Math.Ceiling(sorted.Count * percentile) - 1;
        return sorted[Math.Max(0, Math.Min(index, sorted.Count - 1))];
    }
}

public record PerformanceSnapshot(
    string OperationName,
    double AverageResponseTime,
    double ThroughputPerSecond,
    double ErrorRate,
    double P95ResponseTime);
```

## 8. Monitoring Best Practices

### 1. Metrics Naming Conventions
```
family_{pattern}_{metric_type}_{unit}

Examples:
- family_cqrs_commands_total
- family_event_sourcing_events_stored_total
- family_circuit_breaker_executions_duration_ms
- family_database_operations_duration_ms
```

### 2. Logging Standards
- **Structured Logging**: Verwende JSON-Format für bessere Searchability
- **Correlation IDs**: Verfolge Requests über Service-Grenzen hinweg
- **Log Levels**: DEBUG für Development, INFO für Production
- **Sensitive Data**: Niemals Passwords oder PII in Logs

### 3. Alerting Guidelines
- **Actionable Alerts**: Nur Alerts, die sofortiges Handeln erfordern
- **Clear Descriptions**: Präzise Problem-Beschreibungen
- **Escalation Paths**: Definierte Eskalations-Prozesse
- **Alert Fatigue**: Vermeidung durch sinnvolle Thresholds

### 4. Dashboard Design
- **Business Metrics**: Zeige Business-relevante KPIs
- **Technical Metrics**: Performance und System Health
- **Pattern-Specific Views**: Separate Dashboards pro Pattern
- **Real-time Updates**: Live-Daten für kritische Metrics

Diese umfassende Monitoring-Strategie ermöglicht es, die Cloud Design Patterns effektiv zu überwachen, Performance-Probleme frühzeitig zu erkennen und die Systemgesundheit kontinuierlich zu bewerten.