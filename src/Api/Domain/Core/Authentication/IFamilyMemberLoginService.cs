using Api.Database;
using Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Api.Domain.Core.Authentication;

public interface IFamilyMemberLoginService
{
    Task<LoginProviderAccountResponse> LoginProviderAccount(LoginOidcProviderRequest request, CancellationToken cancellationToken = default);
}

public class LoginProviderAccountResponse : IHasSuccessAndHasError
{
    private LoginProviderAccountResponse()
    {
    }
    
    public Guid? FamilyMemberId { get; set; }
    public string? Name { get; set; }
    public bool IsSuccess { get; private set; }
    public bool IsError { get; private set; }

    public static LoginProviderAccountResponse Success(Guid familyMemberId, string name) => new()
    {
        FamilyMemberId = familyMemberId,
        Name = name,
        IsSuccess = true
    };
    
    public static LoginProviderAccountResponse Error() => new()
    {
        IsError = true
    };
}

public class FamilyMemberLoginService(
    ApplicationDbContext dbContext) : IFamilyMemberLoginService
{
    public async Task<LoginProviderAccountResponse> LoginProviderAccount(LoginOidcProviderRequest request, CancellationToken cancellationToken = default)
    {
        var googleAccount = await dbContext.GoogleAccounts
            .Where(g => g.GoogleId == request.GoogleId)
            .SingleAsync(cancellationToken);

        googleAccount.AccessToken = request.AccessToken;

        await dbContext.SaveChangesAsync(cancellationToken);

        var familyMember = await dbContext.FamilyMembers.AsNoTracking()
            .Where(f => f.AspNetUserId == googleAccount.UserId)
            .Select(f => new
            {
                f.Id,
                Name = $"{f.FirstName} {f.LastName}"
            })
            .SingleAsync(cancellationToken);

        return LoginProviderAccountResponse.Success(familyMember.Id, familyMember.Name);
    }
}