using System.Security.Claims;
using Api.Database;
using Api.Domain.Core.Authentication.Google;
using Api.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Domain.Core.Authentication;

public interface IFamilyMemberRegistrationService
{
    Task<bool> RegisterProviderAccount(RegisterOidcProviderRequest request, CancellationToken cancellationToken = default);
}

public class FamilyMemberRegistrationService(
    ISender sender,
    UserManager<IdentityUser> userManager,
    UsersContext usersContext,
    ApplicationDbContext dbContext) : IFamilyMemberRegistrationService
{
    public async Task<bool> RegisterProviderAccount(RegisterOidcProviderRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.ProviderName.Equals("google", StringComparison.InvariantCultureIgnoreCase))
        {
            throw new NotSupportedException();
        }
        
        ArgumentException.ThrowIfNullOrEmpty(request.EMail);
        ArgumentNullException.ThrowIfNull(request.Birthdate);
        ArgumentException.ThrowIfNullOrEmpty(request.FirstName);
        ArgumentException.ThrowIfNullOrEmpty(request.LastName);
        
        var identityUser = new IdentityUser
        {
            UserName = request.EMail,
            Email = request.EMail
        };

        var result = await userManager.CreateAsync(identityUser);

        if (!result.Succeeded) return false;
        
        var userId = await usersContext.Users
            .Where(user => user.Email == request.EMail)
            .Select(user => user.Id)
            .SingleAsync(cancellationToken);
            
        var familyMemberId = await dbContext.FamilyMembers
            .AsNoTracking()
            .Where(fm => fm.AspNetUserId == userId)
            .Select(fm => fm.Id)
            .SingleAsync(cancellationToken);

        await userManager.AddClaimAsync(identityUser, new Claim(ApplicationClaimNames.CurrentFamilyMemberId, familyMemberId.ToString(), nameof(Guid)));

        // TODO Refactor when using multiple identity providers
        dbContext.GoogleAccounts.Add(new GoogleAccount
        {
            AccessToken = request.GoogleAccessToken,
            GoogleId = request.GoogleId!,
            UserId = userId
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        await sender.Send(request.ToCreateFamilyMemberCommand(userId), cancellationToken);

        return true;

    }
}