using Family.Api.Features.Authentication.Queries;
using Family.Api.GraphQL.Types;
using Family.Api.Services;
using MediatR;

namespace Family.Api.Features.Authentication.Handlers;

public class InitiateLoginHandler : IRequestHandler<InitiateLoginQuery, LoginInitiationPayload>
{
    private readonly IKeycloakService _keycloakService;

    public InitiateLoginHandler(IKeycloakService keycloakService)
    {
        _keycloakService = keycloakService;
    }

    public async Task<LoginInitiationPayload> Handle(InitiateLoginQuery request, CancellationToken cancellationToken)
    {
        var result = await _keycloakService.InitiateLoginAsync();
        return new LoginInitiationPayload(result.LoginUrl, result.State);
    }
}