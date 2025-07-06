using Family.Infrastructure.EventSourcing.Models;
using Family.Infrastructure.Messaging.Abstractions;
using Family.Infrastructure.Messaging.Configuration;
using Family.Infrastructure.Messaging.Models;
using Microsoft.Extensions.Hosting;

namespace Family.Infrastructure.Messaging.Services;

public class KafkaEventBusConsumer : IEventBusConsumer, IHostedService, IDisposable
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IEventSerializer _eventSerializer;
    private readonly KafkaConfiguration _kafkaConfig;
    private readonly ILogger<KafkaEventBusConsumer> _logger;
    private readonly Dictionary<string, List<EventHandler>> _eventHandlers = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private Task? _consumeTask;
    private bool _disposed;

    public KafkaEventBusConsumer(
        IOptions<KafkaConfiguration> kafkaOptions,
        IEventSerializer eventSerializer,
        ILogger<KafkaEventBusConsumer> logger)
    {
        _kafkaConfig = kafkaOptions.Value;
        _eventSerializer = eventSerializer;
        _logger = logger;

        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaConfig.BootstrapServers,
            GroupId = _kafkaConfig.GroupId,
            ClientId = _kafkaConfig.ClientId,
            AutoOffsetReset = _kafkaConfig.AutoOffsetReset,
            EnableAutoCommit = _kafkaConfig.EnableAutoCommit,
            AutoCommitIntervalMs = _kafkaConfig.AutoCommitIntervalMs,
            SecurityProtocol = _kafkaConfig.SecurityProtocol,
            SessionTimeoutMs = 30000,
            HeartbeatIntervalMs = 3000,
            MaxPollIntervalMs = 300000
        };

        _consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, error) => _logger.LogError("Kafka consumer error: {Error}", error.ToString()))
            .SetPartitionsAssignedHandler((_, partitions) => 
                _logger.LogInformation("Assigned partitions: {Partitions}", string.Join(", ", partitions)))
            .SetPartitionsRevokedHandler((_, partitions) => 
                _logger.LogInformation("Revoked partitions: {Partitions}", string.Join(", ", partitions)))
            .Build();
    }

    public void Subscribe<T>(string topic, Func<T, CancellationToken, Task> handler) 
        where T : DomainEvent
    {
        Subscribe(topic, _kafkaConfig.GroupId, handler);
    }

    public void Subscribe<T>(string topic, string consumerGroup, Func<T, CancellationToken, Task> handler) 
        where T : DomainEvent
    {
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Topic cannot be null or empty", nameof(topic));
        
        if (string.IsNullOrWhiteSpace(consumerGroup))
            throw new ArgumentException("Consumer group cannot be null or empty", nameof(consumerGroup));
        
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        var eventHandler = new EventHandler<T>(handler, typeof(T));

        if (!_eventHandlers.TryGetValue(topic, out var handlers))
        {
            handlers = new List<EventHandler>();
            _eventHandlers[topic] = handlers;
        }

        handlers.Add(eventHandler);
        
        _logger.LogInformation("Subscribed to topic {Topic} for event type {EventType} with consumer group {ConsumerGroup}",
            topic, typeof(T).Name, consumerGroup);
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_eventHandlers.Any())
        {
            var topics = _eventHandlers.Keys.ToList();
            _consumer.Subscribe(topics);
            
            _logger.LogInformation("Starting Kafka consumer for topics: {Topics}", string.Join(", ", topics));
            
            _consumeTask = Task.Run(ConsumeLoop, _cancellationTokenSource.Token);
        }
        else
        {
            _logger.LogWarning("No event handlers registered, Kafka consumer will not start");
        }

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stopping Kafka consumer");
        
        _cancellationTokenSource.Cancel();
        
        if (_consumeTask != null)
        {
            await _consumeTask;
        }
        
        _consumer.Close();
        _logger.LogInformation("Kafka consumer stopped");
    }

    private async Task ConsumeLoop()
    {
        try
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(_cancellationTokenSource.Token);
                    
                    if (consumeResult?.Message?.Value != null)
                    {
                        await ProcessMessage(consumeResult);
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message: {Error}", ex.Error);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in consume loop");
                    await Task.Delay(1000, _cancellationTokenSource.Token); // Brief delay before retrying
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in consume loop");
        }
    }

    private async Task ProcessMessage(ConsumeResult<string, string> consumeResult)
    {
        var topic = consumeResult.Topic;
        var message = consumeResult.Message;

        try
        {
            _logger.LogDebug("Processing message from topic {Topic}, partition {Partition}, offset {Offset}",
                topic, consumeResult.Partition, consumeResult.Offset);

            var envelope = JsonSerializer.Deserialize<EventEnvelope>(message.Value);
            if (envelope == null)
            {
                _logger.LogWarning("Failed to deserialize event envelope from topic {Topic}", topic);
                return;
            }

            var eventType = _eventSerializer.GetEventTypeFromName(envelope.EventType);
            if (eventType == null)
            {
                _logger.LogWarning("Unknown event type {EventType} from topic {Topic}", envelope.EventType, topic);
                return;
            }

            var domainEvent = _eventSerializer.Deserialize(envelope.Data, eventType);
            if (domainEvent == null)
            {
                _logger.LogWarning("Failed to deserialize event data for type {EventType} from topic {Topic}", 
                    envelope.EventType, topic);
                return;
            }

            if (_eventHandlers.TryGetValue(topic, out var handlers))
            {
                var applicableHandlers = handlers.Where(h => h.EventType.IsAssignableFrom(eventType)).ToList();
                
                if (applicableHandlers.Any())
                {
                    var tasks = applicableHandlers.Select(async handler =>
                    {
                        try
                        {
                            await handler.HandleAsync(domainEvent, _cancellationTokenSource.Token);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error handling event {EventType} with ID {EventId} from topic {Topic}",
                                envelope.EventType, envelope.EventId, topic);
                            throw;
                        }
                    });

                    await Task.WhenAll(tasks);
                    
                    _logger.LogDebug("Successfully processed event {EventType} with ID {EventId} from topic {Topic}",
                        envelope.EventType, envelope.EventId, topic);
                }
                else
                {
                    _logger.LogDebug("No applicable handlers found for event type {EventType} from topic {Topic}",
                        envelope.EventType, topic);
                }
            }
            else
            {
                _logger.LogDebug("No handlers registered for topic {Topic}", topic);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message from topic {Topic}, partition {Partition}, offset {Offset}",
                topic, consumeResult.Partition, consumeResult.Offset);
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            _cancellationTokenSource.Cancel();
            _consumeTask?.Wait(TimeSpan.FromSeconds(5));
            _consumer?.Close();
            _consumer?.Dispose();
            _cancellationTokenSource.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing Kafka consumer");
        }
        finally
        {
            _disposed = true;
        }
    }

    private abstract class EventHandler
    {
        public abstract Type EventType { get; }
        public abstract Task HandleAsync(DomainEvent domainEvent, CancellationToken cancellationToken);
    }

    private class EventHandler<T> : EventHandler where T : DomainEvent
    {
        private readonly Func<T, CancellationToken, Task> _handler;

        public EventHandler(Func<T, CancellationToken, Task> handler, Type eventType)
        {
            _handler = handler;
            EventType = eventType;
        }

        public override Type EventType { get; }

        public override async Task HandleAsync(DomainEvent domainEvent, CancellationToken cancellationToken)
        {
            if (domainEvent is T typedEvent)
            {
                await _handler(typedEvent, cancellationToken);
            }
        }
    }
}