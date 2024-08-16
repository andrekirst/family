using Api.Domain.Core;
using MediatR;

namespace Api.Infrastructure;

public class SaveDomainEventNotificationHandler<TDomainEvent>(DomainEventRepository domainEventRepository) : INotificationHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    public async Task Handle(TDomainEvent notification, CancellationToken cancellationToken)
    {
        await domainEventRepository.AddAsync(notification, cancellationToken);
    }
}