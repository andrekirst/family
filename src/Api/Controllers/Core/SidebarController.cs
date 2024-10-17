using Api.Features.Core.Sidebar;
using Api.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Core;

[ApiController]
[Route("/api/sidebar")]
public class SidebarController(IMediator mediator) : ApiControllerBase(mediator)
{
    [HttpGet("familymembers")]
    public async Task<IActionResult> FamilyMembers(CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new GetFamilyMembersQuery(), cancellationToken);
        return Ok(result);
    }
}