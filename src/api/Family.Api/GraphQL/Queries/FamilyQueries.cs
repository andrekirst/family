using Family.Api.Authorization;
using Family.Api.Data;
using Family.Api.Features.Families.DTOs;
using Family.Api.Features.Families.Queries;
using Family.Api.GraphQL.Types;
using HotChocolate.Authorization;
using HotChocolate.Types;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Family.Api.GraphQL.Queries;

[ExtendObjectType<Query>]
public class FamilyQueries
{
    [Authorize(Policy = Policies.FamilyUser)]
    public async Task<FamilyDto?> GetMyFamily(
        ClaimsPrincipal claimsPrincipal,
        [Service] IMediator mediator,
        [Service] FamilyDbContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            var subjectId = claimsPrincipal.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(subjectId))
            {
                return null;
            }

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.KeycloakSubjectId == subjectId, cancellationToken);
            if (user == null)
            {
                return null;
            }

            var query = new GetUserFamilyQuery(user.Id);
            return await mediator.Send(query, cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    [Authorize(Policy = Policies.FamilyAdmin)]
    public async Task<FamilyDto?> GetFamilyById(
        string familyId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetFamilyByIdQuery(familyId);
            return await mediator.Send(query, cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    [Authorize(Policy = Policies.FamilyUser)]
    public async Task<bool> HasFamily(
        ClaimsPrincipal claimsPrincipal,
        [Service] IMediator mediator,
        [Service] FamilyDbContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            var subjectId = claimsPrincipal.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(subjectId))
            {
                return false;
            }

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.KeycloakSubjectId == subjectId, cancellationToken);
            if (user == null)
            {
                return false;
            }

            var query = new GetUserFamilyQuery(user.Id);
            var family = await mediator.Send(query, cancellationToken);
            return family != null;
        }
        catch
        {
            return false;
        }
    }
}