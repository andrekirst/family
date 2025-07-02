using Family.Api.GraphQL.Types;
using Family.Api.Services;

namespace Family.Api.GraphQL.Mutations;

[ExtendObjectType<Mutation>]
public class AuthenticationMutations
{
    public async Task<LoginInitiationPayload> InitiateLoginAsync(
        [Service] IKeycloakService keycloakService)
    {
        var result = await keycloakService.InitiateLoginAsync();
        return new LoginInitiationPayload(result.LoginUrl, result.State);
    }

    public async Task<LoginPayload> CompleteLoginAsync(
        LoginCallbackInput input,
        [Service] IKeycloakService keycloakService)
    {
        var result = await keycloakService.CompleteLoginAsync(input.AuthorizationCode, input.State);
        
        if (result.IsSuccess)
        {
            return new LoginPayload(
                result.AccessToken,
                result.RefreshToken,
                result.User,
                null);
        }

        return new LoginPayload(null, null, null, result.Errors);
    }

    public async Task<LoginPayload> DirectLoginAsync(
        LoginInput input,
        [Service] IKeycloakService keycloakService)
    {
        var result = await keycloakService.DirectLoginAsync(input.Email, input.Password);
        
        if (result.IsSuccess)
        {
            return new LoginPayload(
                result.AccessToken,
                result.RefreshToken,
                result.User,
                null);
        }

        return new LoginPayload(null, null, null, result.Errors);
    }

    public async Task<RefreshTokenPayload> RefreshTokenAsync(
        RefreshTokenInput input,
        [Service] IKeycloakService keycloakService)
    {
        var result = await keycloakService.RefreshTokenAsync(input.RefreshToken);
        
        if (result.IsSuccess)
        {
            return new RefreshTokenPayload(
                result.AccessToken,
                result.RefreshToken,
                null);
        }

        return new RefreshTokenPayload(null, null, result.Errors);
    }

    public async Task<LogoutPayload> LogoutAsync(
        string accessToken,
        [Service] IKeycloakService keycloakService)
    {
        var result = await keycloakService.LogoutAsync(accessToken);
        
        return new LogoutPayload(result.IsSuccess, result.Errors);
    }
}

public class Mutation
{
    public string Ping() => "Pong";
}