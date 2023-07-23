using MediatR;

namespace Api.Infrastructure;

public interface IDomainEvent : INotification
{
    public string DomainEventName { get; }
    public int DomainEventVersion { get; }
}