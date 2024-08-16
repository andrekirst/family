using Api.Infrastructure;

namespace Api.Domain.Core;

public class CurrentFamilyMemberIdService(IHttpContextAccessor httpContextAccessor)
{
    public Guid GetFamilyMemberId()
    {
        var claimValue = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(a => a.Type == ApplicationClaimNames.CurrentFamilyMemberId)?.Value;
        
        return claimValue == null
            ? throw new CurrentFamilyMemberIdClaimNotFoundException()
            : Guid.TryParse(claimValue, out var id)
                ? id
                : throw new CurrentFamilyMemberIdClaimInvalidExcpeption(claimValue);
    }
}