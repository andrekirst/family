using Family.Api.Authorization;
using Family.Api.Features.Users.Commands;
using Family.Api.Features.Users.DTOs;
using Family.Infrastructure.CQRS.Abstractions;
using HotChocolate.Authorization;
using MediatR;

namespace Family.Api.GraphQL.Mutations;

/// <summary>
/// GraphQL mutations for user management using CQRS commands
/// </summary>
[ExtendObjectType<Mutation>]
public class UserMutations
{
    /// <summary>
    /// Creates a new user
    /// </summary>
    public async Task<CreateUserPayload> CreateUserAsync(
        CreateUserInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand
        {
            Email = input.Email,
            FirstName = input.FirstName,
            LastName = input.LastName,
            PreferredLanguage = input.PreferredLanguage,
            KeycloakSubjectId = input.KeycloakSubjectId
        };

        var result = await mediator.Send(command, cancellationToken);

        return new CreateUserPayload(result.Data, result.IsSuccess, result.ErrorMessage, result.ValidationErrors);
    }

    /// <summary>
    /// Updates an existing user
    /// </summary>
    public async Task<UpdateUserPayload> UpdateUserAsync(
        UpdateUserInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new UpdateUserCommand
        {
            UserId = input.UserId,
            FirstName = input.FirstName,
            LastName = input.LastName,
            PreferredLanguage = input.PreferredLanguage,
            IsActive = input.IsActive
        };

        var result = await mediator.Send(command, cancellationToken);

        return new UpdateUserPayload(result.Data, result.IsSuccess, result.ErrorMessage, result.ValidationErrors);
    }

    /// <summary>
    /// Deactivates a user (soft delete)
    /// </summary>
    public async Task<DeleteUserPayload> DeleteUserAsync(
        DeleteUserInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new DeleteUserCommand
        {
            UserId = input.UserId
        };

        var result = await mediator.Send(command, cancellationToken);

        return new DeleteUserPayload(result.IsSuccess, result.ErrorMessage, result.ValidationErrors);
    }
}

// Input Types
public record CreateUserInput(
    string Email,
    string FirstName,
    string LastName,
    string PreferredLanguage = "de",
    string? KeycloakSubjectId = null);

public record UpdateUserInput(
    Guid UserId,
    string FirstName,
    string LastName,
    string PreferredLanguage,
    bool IsActive);

public record DeleteUserInput(Guid UserId);

// Payload Types
public record CreateUserPayload(
    UserDto? User,
    bool IsSuccess,
    string? ErrorMessage,
    Dictionary<string, string[]>? ValidationErrors);

public record UpdateUserPayload(
    UserDto? User,
    bool IsSuccess,
    string? ErrorMessage,
    Dictionary<string, string[]>? ValidationErrors);

public record DeleteUserPayload(
    bool IsSuccess,
    string? ErrorMessage,
    Dictionary<string, string[]>? ValidationErrors);