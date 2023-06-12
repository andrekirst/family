using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Infrastructure;

public abstract class ApiControllerBase : ControllerBase
{
    private readonly IMediator _mediator;

    protected ApiControllerBase(IMediator mediator)
    {
        _mediator = mediator;
    }

    protected Task<TResponse> ExecuteQueryAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
        => _mediator.Send(query, cancellationToken);

    protected Task<TResponse> ExecuteCommand<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
        => _mediator.Send(command, cancellationToken);

    protected Task ExecuteCommand(ICommand command, CancellationToken cancellationToken = default)
        => _mediator.Send(command, cancellationToken);
}