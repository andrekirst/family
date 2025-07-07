using Family.Infrastructure.EventSourcing.Models;
using Family.Infrastructure.Messaging.Abstractions;
using Family.Infrastructure.Messaging.Configuration;
using Family.Infrastructure.Messaging.Models;

namespace Family.Infrastructure.Messaging.Services;

public class KafkaEventBusPublisher : IEventBusPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly IEventSerializer _eventSerializer;
    private readonly KafkaConfiguration _kafkaConfig;
    private readonly ILogger<KafkaEventBusPublisher> _logger;
    private bool _disposed;

    public KafkaEventBusPublisher(
        IOptions<KafkaConfiguration> kafkaOptions,
        IEventSerializer eventSerializer,
        ILogger<KafkaEventBusPublisher> logger)
    {
        _kafkaConfig = kafkaOptions.Value;
        _eventSerializer = eventSerializer;
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = _kafkaConfig.BootstrapServers,
            ClientId = _kafkaConfig.ClientId,
            MessageTimeoutMs = _kafkaConfig.MessageTimeoutMs,
            BatchSize = _kafkaConfig.BatchSize,
            LingerMs = _kafkaConfig.LingerMs,
            CompressionType = _kafkaConfig.CompressionType,
            SecurityProtocol = _kafkaConfig.SecurityProtocol,
            Acks = Acks.All,
            EnableIdempotence = true,
            RetryBackoffMs = _kafkaConfig.RetryDelayMs,
            MessageSendMaxRetries = _kafkaConfig.RetryAttempts
        };

        _producer = new ProducerBuilder<string, string>(config)
            .SetErrorHandler((_, error) => _logger.LogError("Kafka producer error: {Error}", error.ToString()))
            .Build();
    }

    public async Task PublishAsync<T>(T @event, string? topic = null, CancellationToken cancellationToken = default) 
        where T : DomainEvent
    {
        var resolvedTopic = topic ?? GetDefaultTopicForEvent(@event);
        var key = @event.AggregateId;
        
        await PublishAsync(@event, resolvedTopic, key, cancellationToken);
    }

    public async Task PublishAsync<T>(T @event, string topic, string? key = null, CancellationToken cancellationToken = default) 
        where T : DomainEvent
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));
        
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Topic cannot be null or empty", nameof(topic));

        try
        {
            var envelope = CreateEventEnvelope(@event, topic, key);
            var serializedEnvelope = JsonSerializer.Serialize(envelope);
            
            var message = new Message<string, string>
            {
                Key = envelope.Key ?? envelope.AggregateId,
                Value = serializedEnvelope,
                Headers = CreateHeaders(@event)
            };

            _logger.LogDebug("Publishing event {EventType} with ID {EventId} to topic {Topic}", 
                @event.EventType, @event.EventId, topic);

            var deliveryResult = await _producer.ProduceAsync(topic, message, cancellationToken);
            
            _logger.LogInformation("Successfully published event {EventType} with ID {EventId} to topic {Topic}, partition {Partition}, offset {Offset}",
                @event.EventType, @event.EventId, topic, deliveryResult.Partition, deliveryResult.Offset);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType} with ID {EventId} to topic {Topic}: {Error}",
                @event.EventType, @event.EventId, topic, ex.Error);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error publishing event {EventType} with ID {EventId} to topic {Topic}",
                @event.EventType, @event.EventId, topic);
            throw;
        }
    }

    public async Task PublishBatchAsync<T>(IEnumerable<T> events, string? topic = null, CancellationToken cancellationToken = default) 
        where T : DomainEvent
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        var eventList = events?.ToList() ?? throw new ArgumentNullException(nameof(events));
        
        if (!eventList.Any())
            return;

        var tasks = eventList.Select(async @event =>
        {
            var resolvedTopic = topic ?? GetDefaultTopicForEvent(@event);
            await PublishAsync(@event, resolvedTopic, cancellationToken: cancellationToken);
        });

        await Task.WhenAll(tasks);
        
        _logger.LogInformation("Successfully published batch of {Count} events", eventList.Count);
    }

    private EventEnvelope CreateEventEnvelope<T>(T @event, string topic, string? key) where T : DomainEvent
    {
        var serializedData = _eventSerializer.Serialize(@event);
        
        return new EventEnvelope
        {
            EventId = @event.EventId,
            EventType = @event.EventType,
            AggregateId = @event.AggregateId,
            AggregateType = @event.AggregateType,
            Version = @event.Version,
            Timestamp = @event.Timestamp,
            UserId = @event.UserId,
            CorrelationId = @event.CorrelationId,
            CausationId = @event.CausationId,
            Data = serializedData,
            Metadata = @event.Metadata,
            Topic = topic,
            Key = key,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static Headers CreateHeaders<T>(T @event) where T : DomainEvent
    {
        var headers = new Headers
        {
            { "eventType", System.Text.Encoding.UTF8.GetBytes(@event.EventType) },
            { "aggregateType", System.Text.Encoding.UTF8.GetBytes(@event.AggregateType) },
            { "aggregateId", System.Text.Encoding.UTF8.GetBytes(@event.AggregateId) },
            { "eventId", System.Text.Encoding.UTF8.GetBytes(@event.EventId) },
            { "correlationId", System.Text.Encoding.UTF8.GetBytes(@event.CorrelationId) },
            { "causationId", System.Text.Encoding.UTF8.GetBytes(@event.CausationId) },
            { "userId", System.Text.Encoding.UTF8.GetBytes(@event.UserId) },
            { "timestamp", System.Text.Encoding.UTF8.GetBytes(@event.Timestamp.ToString("O")) },
            { "version", System.Text.Encoding.UTF8.GetBytes(@event.Version.ToString()) }
        };

        return headers;
    }

    private string GetDefaultTopicForEvent<T>(T @event) where T : DomainEvent
    {
        // Route events to appropriate topics based on aggregate type or event type
        return @event.AggregateType.ToLowerInvariant() switch
        {
            "family" => _kafkaConfig.Topics.FamilyEvents,
            "user" => _kafkaConfig.Topics.UserEvents,
            _ => _kafkaConfig.Topics.IntegrationEvents
        };
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            _producer?.Flush(TimeSpan.FromSeconds(10));
            _producer?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing Kafka producer");
        }
        finally
        {
            _disposed = true;
        }
    }
}