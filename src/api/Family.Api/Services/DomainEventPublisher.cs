using Family.Infrastructure.EventSourcing.Models;
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

    public DomainEventPublisher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        // Only publish events that implement INotification
        if (domainEvent is INotification notification)
        {
            await _mediator.Publish(notification, cancellationToken);
        }
    }

    public async Task PublishAllAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        var tasks = domainEvents
            .OfType<INotification>()
            .Select(notification => _mediator.Publish(notification, cancellationToken));

        await Task.WhenAll(tasks);
    }
}