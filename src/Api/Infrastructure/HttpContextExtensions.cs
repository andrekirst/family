using Api.Domain.Core;

namespace Api.Infrastructure;

public static class HttpContextExtensions
{
    public static Guid GetFamilyMemberId(this HttpContext httpContext)
    {
        var claimValue = httpContext.User.Claims.FirstOrDefault(a => a.Type == ApplicationClaimNames.FamilyMemberId)?.Value;

        return claimValue == null
            ? throw new CurrentFamilyMemberIdClaimNotFoundException()
            : Guid.TryParse(claimValue, out var id)
                ? id
                : throw new CurrentFamilyMemberIdClaimInvalidExcpeption(claimValue);
    }
}