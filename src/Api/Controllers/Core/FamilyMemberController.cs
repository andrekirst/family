﻿using Api.Domain.Core;
using Api.Features.Core;
using Api.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Core;

[ApiController]
[Route("api/core/[controller]")]
[Authorize]
public class FamilyMemberController : ApiControllerBase
{
    public FamilyMemberController(IMediator mediator)
        : base(mediator)
    {
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<GetFamilyMembersQueryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var result = await ExecuteQueryAsync(new GetFamilyMembersQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFamilyMemberCommandModel model, CancellationToken cancellationToken = default)
    {
        await ExecuteCommand(new CreateFamilyMemberCommand(model), cancellationToken);
        return Ok();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFamilyMemberCommandModel model, CancellationToken cancellationToken = default)
    {
        await ExecuteCommand(new UpdateFamilyMemberCommand(id, model), cancellationToken);
        return Ok();
    }
}