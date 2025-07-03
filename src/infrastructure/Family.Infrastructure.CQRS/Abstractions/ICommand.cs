namespace Family.Infrastructure.CQRS.Abstractions;

/// <summary>
/// Represents a command that modifies state and returns no result
/// </summary>
public interface ICommand : IRequest
{
}

/// <summary>
/// Represents a command that modifies state and returns a result
/// </summary>
/// <typeparam name="TResult">The type of result returned by the command</typeparam>
public interface ICommand<out TResult> : IRequest<TResult>
{
}

/// <summary>
/// Represents a command handler that processes commands without returning a result
/// </summary>
/// <typeparam name="TCommand">The type of command to handle</typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand>
    where TCommand : ICommand
{
}

/// <summary>
/// Represents a command handler that processes commands and returns a result
/// </summary>
/// <typeparam name="TCommand">The type of command to handle</typeparam>
/// <typeparam name="TResult">The type of result returned by the command</typeparam>
public interface ICommandHandler<in TCommand, TResult> : IRequestHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
}