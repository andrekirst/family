using Api.Database;
using Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Api.Domain.Core.Authentication;

public interface IFamilyMemberLoginService
{
    Task<Result<LoginProviderAccountResponse>> LoginProviderAccount(LoginOidcProviderRequest request, CancellationToken cancellationToken = default);
}

public class LoginProviderAccountResponse
{
    public Guid? FamilyMemberId { get; init; }
    public string? Name { get; init; }
}

public class FamilyMemberLoginService(ApplicationDbContext dbContext) : IFamilyMemberLoginService
{
    public async Task<Result<LoginProviderAccountResponse>> LoginProviderAccount(LoginOidcProviderRequest request, CancellationToken cancellationToken = default)
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

        return new LoginProviderAccountResponse
        {
            FamilyMemberId = familyMember.Id,
            Name = familyMember.Name
        };
    }
}