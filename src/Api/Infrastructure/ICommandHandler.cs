using MediatR;

namespace Api.Infrastructure;

public interface ICommandHandler<in TRequst> : IRequestHandler<TRequst>
    where TRequst : ICommand
{
}