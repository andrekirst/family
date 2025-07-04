using Family.Api.Features.Authentication.Commands;
using Family.Api.GraphQL.Types;
using Family.Api.Services;
using MediatR;

namespace Family.Api.Features.Authentication.Handlers;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenPayload>
{
    private readonly IKeycloakService _keycloakService;

    public RefreshTokenHandler(IKeycloakService keycloakService)
    {
        _keycloakService = keycloakService;
    }

    public async Task<RefreshTokenPayload> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var result = await _keycloakService.RefreshTokenAsync(request.RefreshToken);

        if (result.IsSuccess)
        {
            return RefreshTokenPayload.Success(result.AccessToken!, result.RefreshToken);
        }

        return RefreshTokenPayload.Failure(result.Errors);
    }
}