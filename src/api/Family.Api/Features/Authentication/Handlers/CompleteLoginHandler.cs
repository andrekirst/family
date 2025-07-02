using Family.Api.Features.Authentication.Commands;
using Family.Api.GraphQL.Types;
using Family.Api.Services;
using MediatR;

namespace Family.Api.Features.Authentication.Handlers;

public class CompleteLoginHandler : IRequestHandler<CompleteLoginCommand, LoginPayload>
{
    private readonly IKeycloakService _keycloakService;

    public CompleteLoginHandler(IKeycloakService keycloakService)
    {
        _keycloakService = keycloakService;
    }

    public async Task<LoginPayload> Handle(CompleteLoginCommand request, CancellationToken cancellationToken)
    {
        var result = await _keycloakService.CompleteLoginAsync(request.AuthorizationCode, request.State);

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
}