using Family.Api.Authorization;
using Family.Api.Data;
using Family.Api.Features.Families.Commands;
using Family.Api.GraphQL.Types;
using HotChocolate.Authorization;
using HotChocolate.Types;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Family.Api.GraphQL.Mutations;

[ExtendObjectType<Mutation>]
public class FamilyMutations
{
    [Authorize(Policy = Policies.FamilyUser)]
    public async Task<CreateFamilyPayload> CreateFamily(
        CreateFamilyInput input,
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
                return new CreateFamilyPayload
                {
                    Success = false,
                    ErrorMessage = "User not found"
                };
            }

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.KeycloakSubjectId == subjectId, cancellationToken);
            if (user == null)
            {
                return new CreateFamilyPayload
                {
                    Success = false,
                    ErrorMessage = "User not found"
                };
            }

            var command = new CreateFamilyCommand(
                input.Name,
                user.Id,
                Guid.NewGuid().ToString());

            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                return new CreateFamilyPayload
                {
                    Success = true,
                    Family = result.Family
                };
            }
            else
            {
                var validationErrors = result.ValidationErrors?.SelectMany(kvp => kvp.Value).ToList();
                return new CreateFamilyPayload
                {
                    Success = false,
                    ErrorMessage = result.ErrorMessage,
                    ValidationErrors = validationErrors
                };
            }
        }
        catch (Exception ex)
        {
            return new CreateFamilyPayload
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}