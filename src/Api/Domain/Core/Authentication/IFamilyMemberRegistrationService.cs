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
    /// <summary>
    /// Registering a new User, FamilyMember and GoogleAccount
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The AspNetUserId</returns>
    Task<RegisterProviderAccountResponse> RegisterProviderAccount(RegisterOidcProviderRequest request, CancellationToken cancellationToken = default);
}

public class RegisterProviderAccountResponse : IHasSuccessAndHasError
{
    private RegisterProviderAccountResponse()
    {
    }
    
    public string? AspNetUserId { get; set; }
    public Guid? FamilyMemberId { get; set; }
    public IdentityUser? IdentityUser { get; set; }
    public bool IsSuccess { get; private set; }
    public bool IsError { get; private set; }

    public static RegisterProviderAccountResponse Error() => new()
    {
        IsError = true
    };

    public static RegisterProviderAccountResponse Success(string aspNetUserId, Guid familyMemberId, IdentityUser identityUser) => new()
    {
        IsSuccess = true,
        AspNetUserId = aspNetUserId,
        FamilyMemberId = familyMemberId,
        IdentityUser = identityUser
    };
}

public class FamilyMemberRegistrationService(
    ISender sender,
    UserManager<IdentityUser> userManager,
    UsersContext usersContext,
    ApplicationDbContext dbContext) : IFamilyMemberRegistrationService
{
    public async Task<RegisterProviderAccountResponse> RegisterProviderAccount(RegisterOidcProviderRequest request, CancellationToken cancellationToken = default)
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

        if (!result.Succeeded)
        {
            return RegisterProviderAccountResponse.Error();
        }
        
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

        return RegisterProviderAccountResponse.Success(userId, familyMemberId, identityUser);
    }
}