using System.Text;
using System.Text.Json;
using Api.Extensions;
using Confluent.Kafka;
using MediatR;

namespace Api.Infrastructure.DomainEvents;

public class DomainEventsConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<DomainEventsConsumer> _logger;
    private readonly IConsumer<Ignore, string> _consumer;
    private static readonly Type DomainEventType = typeof(IDomainEvent); 

    public DomainEventsConsumer(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<DomainEventsConsumer> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        
        // TODO Als Options
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "DomainEventsGroup",
            AllowAutoCreateTopics = true,
            EnableAutoCommit = false
        };
        
        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        _consumer.Subscribe("DomainEvents");
    }

    private static (Type Type, DomainEventAttribute Attribute)[]? _cachedImplementations;
    
    private static Type? ImplementationOfIDomainEvent(string name)
    {
        _cachedImplementations ??= DomainEventType
            .Assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && DomainEventType.IsAssignableFrom(type))
            .Select(type => (type, (DomainEventAttribute)Attribute.GetCustomAttribute(type, typeof(DomainEventAttribute))!))
            .ToArray();

        return _cachedImplementations
            .Where(t => t.Attribute.Name == name)
            .Select(t => t.Type)
            .FirstOrDefault();
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(async () =>
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

            while (!stoppingToken.IsCancellationRequested)
            {
                var result = _consumer.Consume(stoppingToken);

                if (result == null)
                {
                    await Task.Delay(1.Seconds(), stoppingToken);
                    continue;
                }

                var hasDomainEventName = result.Message.Headers.TryGetLastBytes(DomainEventHeaderKeys.Name, out var domainEventNameValue);

                if (!hasDomainEventName)
                {
                    _logger.LogWarning("Message had no header value for {name}", DomainEventHeaderKeys.Name);
                    continue;
                }

                var domainEventName = Encoding.Default.GetString(domainEventNameValue);
                var type = ImplementationOfIDomainEvent(domainEventName);

                if (type == null)
                {
                    _logger.LogWarning("Could not find a type of {type} for domain event name {domainEventName}", type, domainEventName);
                    continue;
                }

                if (JsonSerializer.Deserialize(result.Message.Value, type) is not IDomainEvent domainEvent)
                {
                    _logger.LogWarning("Could not cast message to implementation");
                    continue;
                }

                await publisher.Publish(domainEvent, stoppingToken);
                _consumer.Commit(result);
                await Task.Delay(1.Seconds(), stoppingToken);
            }
        }, stoppingToken);
    }

    public override void Dispose()
    {
        _consumer.Dispose();
    }
}