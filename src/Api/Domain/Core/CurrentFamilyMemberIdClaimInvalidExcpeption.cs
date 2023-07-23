namespace Api.Domain.Core;

public class CurrentFamilyMemberIdClaimInvalidExcpeption : Exception
{
    public string? ClaimValue { get; }

    public CurrentFamilyMemberIdClaimInvalidExcpeption(string? claimValue)
    {
        ClaimValue = claimValue;
        throw new NotImplementedException();
    }
}