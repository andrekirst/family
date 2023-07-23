using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Infrastructure;

public abstract class ApiControllerBase : ControllerBase
{
    protected ApiControllerBase(IMediator mediator)
    {
        Mediator = mediator;
    }

    protected IMediator Mediator { get; }

    protected Task<TResponse> ExecuteQueryAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
        => Mediator.Send(query, cancellationToken);

    protected Task<TResponse> ExecuteCommand<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
        => Mediator.Send(command, cancellationToken);

    protected Task ExecuteCommand(ICommand command, CancellationToken cancellationToken = default)
        => Mediator.Send(command, cancellationToken);
}