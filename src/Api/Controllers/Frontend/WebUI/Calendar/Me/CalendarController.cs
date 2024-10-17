using System.Net;
using Api.Features.Frontend.WebUI.Calendar.Me;
using Api.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Frontend.WebUI.Calendar.Me;

[ApiController]
[Route("/api/frontend/webui/calendar/me/calendar")]
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