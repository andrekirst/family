using System.Net;
using Api.Features.Calendar.Me;
using Api.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Calendar.Me;

[ApiController]
[Route("/api/calendar/me/")]
public class CalendarController(IMediator mediator) : ApiControllerBase(mediator)
{
    [HttpGet("list")]
    [ProducesResponseType<GetCalendarMeListQueryResponse>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken = default)
    {
        var query = new GetCalendarMeListQuery();
        var result = await ExecuteQueryAsync(query, cancellationToken);
        return HandleDefaultResult(result);
    }
}