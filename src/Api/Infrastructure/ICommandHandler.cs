using Api.Infrastructure.DomainEvents;
using MediatR;

namespace Api.Infrastructure;

public interface ICommandHandler<in TRequest> : IRequestHandler<TRequest>
    where TRequest : ICommand;

public interface ICommandHandler<in TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>;


public abstract class CommandHandler<TRequest, TResponse>(
    IHttpContextAccessor httpContextAccessor,
    IUnitOfWork unitOfWork,
    IDomainEventBus domainEventBus)
    : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);

    protected HttpContext HttpContext => httpContextAccessor.HttpContext!;
    protected Guid FamilyMemberId => httpContextAccessor.HttpContext!.GetFamilyMemberId();
    protected Task SaveChangesAsync(CancellationToken cancellationToken = default) => unitOfWork.SaveChangesAsync(cancellationToken);

    protected Task SendToDomainEventBus<TDomainEvent>(TDomainEvent domainEvent, CancellationToken cancellationToken = default)
        where TDomainEvent : IDomainEvent =>
        domainEventBus.SendAsync(domainEvent, cancellationToken);
}