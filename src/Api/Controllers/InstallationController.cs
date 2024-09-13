using System.Net;
using Api.Domain.Installation.Install;
using Api.Domain.Installation.IsInstalled;
using Api.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class InstallationController(IMediator mediator) : ApiControllerBase(mediator)
{
    [HttpGet("is-installed")]
    [ProducesResponseType<bool>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> IsInstalled()
    {
        var isInstalled = await ExecuteQueryAsync(new IsInstalledQuery());
        return Ok(isInstalled);
    }

    [HttpPost("install")]
    public async Task<IActionResult> Install([FromBody] InstallationOptions options, CancellationToken cancellationToken = default)
    {
        var command = new InstallCommand(options);
        var result = await ExecuteCommandAsync(command, cancellationToken);
        return result ? Ok() : BadRequest();
    }
}