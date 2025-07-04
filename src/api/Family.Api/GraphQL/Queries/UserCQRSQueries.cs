using Family.Api.Authorization;
using Family.Api.Features.Users.DTOs;
using Family.Api.Features.Users.Queries;
using HotChocolate.Authorization;
using MediatR;

namespace Family.Api.GraphQL.Queries;

/// <summary>
/// GraphQL queries for user management using CQRS queries
/// </summary>
[ExtendObjectType<Query>]
public class UserCQRSQueries
{
    /// <summary>
    /// Gets a user by ID
    /// </summary>
    public async Task<UserDto?> GetUserByIdAsync(
        Guid userId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery { UserId = userId };
        return await mediator.Send(query, cancellationToken);
    }

    /// <summary>
    /// Gets a user by email address
    /// </summary>
    public async Task<UserDto?> GetUserByEmailAsync(
        string email,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetUserByEmailQuery { Email = email };
        return await mediator.Send(query, cancellationToken);
    }

    /// <summary>
    /// Gets all users with optional pagination
    /// </summary>
    public async Task<List<UserDto>> GetAllUsersAsync(
        [Service] IMediator mediator,
        int pageNumber = 1,
        int pageSize = 50,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllUsersQuery 
        { 
            PageNumber = pageNumber, 
            PageSize = pageSize,
            IncludeInactive = includeInactive
        };
        
        return await mediator.Send(query, cancellationToken);
    }
}