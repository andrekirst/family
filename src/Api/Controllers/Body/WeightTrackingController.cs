using Api.Domain.Body.WeightTracking;
using Api.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Body;

[Route("api/body/familymember/{familyMemberId:guid}/[controller]")]
[ApiController]
[Authorize]
public class WeightTrackingController(IMediator mediator) : ApiControllerBase(mediator)
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(Guid familyMemberId, [FromBody] CreateWeightTrackingEntryCommandModel model, CancellationToken cancellationToken = default)
    {
        await ExecuteCommandAsync(new CreateWeightTrackingEntryCommand(familyMemberId, model), cancellationToken);
        return Ok();
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, Guid familyMemberId, [FromBody] UpdateWeightTrackingEntryCommandModel model, CancellationToken cancellationToken = default)
    {
        await ExecuteCommandAsync(new UpdateWeightTrackingEntryCommand(id, familyMemberId, model), cancellationToken);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, Guid familyMemberId, CancellationToken cancellationToken = default)
    {
        await ExecuteCommandAsync(new DeleteWeightTrackingEntryCommand(id, familyMemberId), cancellationToken);
        return Ok();
    }
}