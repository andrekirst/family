using Family.Api.Features.Authentication.Commands;
using Family.Api.GraphQL.Types;
using Family.Api.Services;
using MediatR;

namespace Family.Api.Features.Authentication.Handlers;

public class LogoutHandler : IRequestHandler<LogoutCommand, LogoutPayload>
{
    private readonly IKeycloakService _keycloakService;

    public LogoutHandler(IKeycloakService keycloakService)
    {
        _keycloakService = keycloakService;
    }

    public async Task<LogoutPayload> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var result = await _keycloakService.LogoutAsync(request.AccessToken);

        return new LogoutPayload(result.IsSuccess, result.Errors);
    }
}