using Api.Domain.Body.WeightTracking;
using Api.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Body;

[Route("api/body/[controller]")]
[ApiController]
[Authorize]
public class WeightTrackingController : ApiControllerBase
{
    public WeightTrackingController(IMediator mediator)
        : base(mediator)
    {
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateWeightTrackingEntryCommandModel model, CancellationToken cancellationToken = default)
    {
        await ExecuteCommand(new CreateWeightTrackingEntryCommand(model), cancellationToken);
        return Ok();
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWeightTrackingEntryCommandModel model, CancellationToken cancellationToken = default)
    {
        await ExecuteCommand(new UpdateWeightTrackingEntryCommand(id, model), cancellationToken);
        return Ok();
    }

    [HttpDelete("familymember/{familyMemberId:int}/entry/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(int id, int familyMemberId, CancellationToken cancellationToken = default)
    {
        await ExecuteCommand(new DeleteWeightTrackingEntryCommand(id, familyMemberId), cancellationToken);
        return Ok();
    }
}