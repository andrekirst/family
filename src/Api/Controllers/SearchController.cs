using Api.Domain.Search;
using Api.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SearchController(IMediator mediator) : ApiControllerBase(mediator)
{
    [HttpGet]
    public IAsyncEnumerable<SearchResult> Get(string value, CancellationToken cancellationToken = default)
        => Mediator.CreateStream(new SearchDataQuery(value), cancellationToken);
}