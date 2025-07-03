namespace Family.Infrastructure.CQRS.Abstractions;

/// <summary>
/// Represents a query that retrieves data and returns a result
/// </summary>
/// <typeparam name="TResult">The type of result returned by the query</typeparam>
public interface IQuery<out TResult> : IRequest<TResult>
{
}

/// <summary>
/// Represents a query handler that processes queries and returns a result
/// </summary>
/// <typeparam name="TQuery">The type of query to handle</typeparam>
/// <typeparam name="TResult">The type of result returned by the query</typeparam>
public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
}