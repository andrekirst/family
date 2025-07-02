using Family.Api.Features.Authentication.Commands;
using Family.Api.Features.Authentication.Queries;
using Family.Api.GraphQL.Types;
using MediatR;

namespace Family.Api.GraphQL.Mutations;

[ExtendObjectType<Mutation>]
public class AuthenticationMutations
{
    public async Task<LoginInitiationPayload> InitiateLoginAsync(
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(new InitiateLoginQuery(), cancellationToken);
    }

    public async Task<LoginPayload> CompleteLoginAsync(
        LoginCallbackInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(new CompleteLoginCommand(input.AuthorizationCode, input.State), cancellationToken);
    }

    public async Task<LoginPayload> DirectLoginAsync(
        LoginInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(new DirectLoginCommand(input.Email, input.Password), cancellationToken);
    }

    public async Task<RefreshTokenPayload> RefreshTokenAsync(
        RefreshTokenInput input,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(new RefreshTokenCommand(input.RefreshToken), cancellationToken);
    }

    public async Task<LogoutPayload> LogoutAsync(
        string accessToken,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await mediator.Send(new LogoutCommand(accessToken), cancellationToken);
    }
}

public class Mutation
{
    public string Ping() => "Pong";
}