using Api.Domain.Core.Authentication;
using Api.Domain.Core.Authentication.Credentials;
using Api.Domain.Core.Authentication.Google;
using Api.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IMediator mediator) : ApiControllerBase(mediator)
{
    [HttpPost]
    [Route("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegistrationRequest request, CancellationToken cancellationToken = default)
    {
        var command = new RegisterCommand(request);
        var result = await Mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Created()
            : BadRequest(result.Error);
    }

    [HttpPost]
    [Route("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
    {
        var command = new LoginCommand(request);
        var result = await Mediator.Send(command, cancellationToken);
        
        return result.IsSuccess
            ? Ok(result.Value)
            : Unauthorized();
    }

    [HttpPost, Route("google-login"), AllowAnonymous, Produces<LoginResponse>]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request, CancellationToken cancellationToken = default)
    {
        var command = new GoogleLoginCommand(request);
        var result = await Mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : Unauthorized(result.Error);
    }
}