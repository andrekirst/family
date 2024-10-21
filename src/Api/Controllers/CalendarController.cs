using System.Net;
using Api.Features.Calendar.Me;
using Api.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("/api/[controller]/")]
public class CalendarController(IMediator mediator) : ApiControllerBase(mediator)
{
    [HttpGet("me/list")]
    [ProducesResponseType<GetCalendarMeListQueryResponse>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> MeList(CancellationToken cancellationToken = default)
    {
        var query = new GetCalendarMeListQuery();
        var result = await ExecuteQueryAsync(query, cancellationToken);
        return HandleDefaultResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCalendar([FromBody] CreateCalendarRequest request, CancellationToken cancellationToken = default)
    {
        var command = new CreateCalendarCommand(request);
        var result = await ExecuteCommandAsync(command, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetCalendar), new { id = result.Value!.CalendarId }, result.Value!.CalendarId)
            : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    public Task<IActionResult> GetCalendar(Guid id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}