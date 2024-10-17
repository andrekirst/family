using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Infrastructure;

public abstract class ApiControllerBase(IMediator mediator) : ControllerBase
{
    protected IMediator Mediator { get; } = mediator;

    protected Task<TResponse> ExecuteQueryAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
        => Mediator.Send(query, cancellationToken);

    protected Task<TResponse> ExecuteCommandAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
        => Mediator.Send(command, cancellationToken);

    protected Task ExecuteCommandAsync(ICommand command, CancellationToken cancellationToken = default)
        => Mediator.Send(command, cancellationToken);

    protected IActionResult HandleDefaultResult<TValue, TError>(Result<TValue, TError> result)
    {
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error);
    }
}