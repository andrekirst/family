using Family.Api.Features.Authentication.Commands;
using Family.Api.GraphQL.Types;
using Family.Api.Services;
using MediatR;

namespace Family.Api.Features.Authentication.Handlers;

public class DirectLoginHandler : IRequestHandler<DirectLoginCommand, LoginPayload>
{
    private readonly IKeycloakService _keycloakService;

    public DirectLoginHandler(IKeycloakService keycloakService)
    {
        _keycloakService = keycloakService;
    }

    public async Task<LoginPayload> Handle(DirectLoginCommand request, CancellationToken cancellationToken)
    {
        var result = await _keycloakService.DirectLoginAsync(request.Email, request.Password);

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