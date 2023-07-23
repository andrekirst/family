using Api.Infrastructure;

namespace Api.Domain.Core;

public class CurrentFamilyMemberIdService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentFamilyMemberIdService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int GetFamilyMemberId()
    {
        var claimValue = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(a => a.Type == ApplicationClaimNames.CurrentFamilyMemberId)?.Value;

        return claimValue == null
            ? throw new CurrentFamilyMemberIdClaimNotFoundException()
            : int.TryParse(claimValue, out var id)
                ? id
                : throw new CurrentFamilyMemberIdClaimInvalidExcpeption(claimValue);
    }
}