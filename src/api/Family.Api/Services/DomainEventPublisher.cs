using Family.Infrastructure.EventSourcing.Models;
using Family.Infrastructure.Messaging.Abstractions;
using MediatR;

namespace Family.Api.Services;

public interface IDomainEventPublisher
{
    Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default);
    Task PublishAllAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default);
}

public class DomainEventPublisher : IDomainEventPublisher
{
    private readonly IMediator _mediator;
    private readonly IEventBusPublisher _eventBusPublisher;
    private readonly ILogger<DomainEventPublisher> _logger;

    public DomainEventPublisher(
        IMediator mediator,
        IEventBusPublisher eventBusPublisher,
        ILogger<DomainEventPublisher> logger)
    {
        _mediator = mediator;
        _eventBusPublisher = eventBusPublisher;
        _logger = logger;
    }

    public async Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        // Publish to local in-process handlers via MediatR
        if (domainEvent is INotification notification)
        {
            await _mediator.Publish(notification, cancellationToken);
        }

        // Publish to external systems via Kafka
        try
        {
            await _eventBusPublisher.PublishAsync(domainEvent, cancellationToken: cancellationToken);
            
            _logger.LogDebug("Published domain event {EventType} with ID {EventId} to event bus",
                domainEvent.EventType, domainEvent.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish domain event {EventType} with ID {EventId} to event bus",
                domainEvent.EventType, domainEvent.EventId);
            
            // Don't rethrow - local processing has already succeeded
            // Consider implementing a dead letter queue or retry mechanism
        }
    }

    public async Task PublishAllAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        var eventList = domainEvents?.ToList() ?? new List<DomainEvent>();
        
        if (!eventList.Any())
            return;

        // Publish to local in-process handlers via MediatR
        var mediatorTasks = eventList
            .OfType<INotification>()
            .Select(notification => _mediator.Publish(notification, cancellationToken));

        await Task.WhenAll(mediatorTasks);

        // Publish to external systems via Kafka
        try
        {
            await _eventBusPublisher.PublishBatchAsync(eventList, cancellationToken: cancellationToken);
            
            _logger.LogDebug("Published batch of {Count} domain events to event bus", eventList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish batch of {Count} domain events to event bus", eventList.Count);
            
            // Don't rethrow - local processing has already succeeded
            // Consider implementing a dead letter queue or retry mechanism
        }
    }
}