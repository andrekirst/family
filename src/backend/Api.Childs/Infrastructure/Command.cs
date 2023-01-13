using MediatR;

namespace Api.Childs.Infrastructure;

public abstract class Command<TResponse>  : IRequest<TResponse>
{
}