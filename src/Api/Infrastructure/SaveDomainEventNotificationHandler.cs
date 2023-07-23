using Api.Domain.Core;
using MediatR;

namespace Api.Infrastructure;

public class SaveDomainEventNotificationHandler<TDomainEvent> : INotificationHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    private readonly DomainEventRepository _domainEventRepository;

    public SaveDomainEventNotificationHandler(DomainEventRepository domainEventRepository)
    {
        _domainEventRepository = domainEventRepository;
    }

    public async Task Handle(TDomainEvent notification, CancellationToken cancellationToken)
    {
        await _domainEventRepository.AddAsync(notification, cancellationToken);
    }
}