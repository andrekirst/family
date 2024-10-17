namespace Api.Infrastructure;

public static class HttpContextExtensions
{
    public static Result<Guid> GetFamilyMemberId(this HttpContext httpContext)
    {
        var claimValue = httpContext.User.Claims.FirstOrDefault(a => a.Type == ApplicationClaimNames.FamilyMemberId)?.Value;

        return claimValue == null
            ? Errors.NoHttpContext
            : Guid.TryParse(claimValue, out var id)
                ? id
                : Errors.InvalidFamilyMemberIdFormat;
    }
}