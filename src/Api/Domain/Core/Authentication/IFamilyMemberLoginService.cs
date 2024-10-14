using Api.Database;
using Microsoft.EntityFrameworkCore;

namespace Api.Domain.Core.Authentication;

public interface IFamilyMemberLoginService
{
    Task<bool> LoginProviderAccount(LoginOidcProviderRequest request, CancellationToken cancellationToken = default);
}

public class FamilyMemberLoginService(ApplicationDbContext dbContext) : IFamilyMemberLoginService
{
    public async Task<bool> LoginProviderAccount(LoginOidcProviderRequest request, CancellationToken cancellationToken = default)
    {
        var googleAccount = await dbContext.GoogleAccounts
            .Where(g => g.GoogleId == request.GoogleId)
            .SingleAsync(cancellationToken);

        googleAccount.AccessToken = request.AccessToken;

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}