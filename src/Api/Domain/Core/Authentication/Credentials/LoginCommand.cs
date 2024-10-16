using System.Security.Claims;
using Api.Database;
using Api.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Domain.Core.Authentication.Credentials;

public record LoginCommand(LoginRequest Request) : ICommand<Result<LoginResponse, Error>>;

public class LoginCommandHandler(
    ApplicationDbContext dbContext,
    ITokenService tokenService,
    UserManager<IdentityUser> userManager)
    : ICommandHandler<LoginCommand, Result<LoginResponse, Error>>
{
    public async Task<Result<LoginResponse, Error>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        
        var managedUser = await userManager.FindByEmailAsync(request.Login) ??
                          await userManager.FindByNameAsync(request.Login);

        if (managedUser == null)
        {
            return Errors.Authentication.BadCredentials();
        }

        var isPasswordValid = await userManager.CheckPasswordAsync(managedUser, request.Password);
        if (!isPasswordValid)
        {
            await userManager.AccessFailedAsync(managedUser);
            return Errors.Authentication.BadCredentials();
        }

        var familyMember = await dbContext.FamilyMembers
            .AsNoTracking()
            .Where(fm => fm.AspNetUserId == managedUser.Id)
            .Select(fm => new
            {
                fm.Id,
                Name = $"{fm.FirstName} {fm.LastName}"
            })
            .SingleAsync(cancellationToken);

        var addClaimsResult = await userManager.AddClaimAsync(
            managedUser,
            new Claim(ApplicationClaimNames.CurrentFamilyMemberId,
                familyMember.Id.ToString(),
                nameof(Guid)));

        await userManager.ResetAccessFailedCountAsync(managedUser);

        var accessToken = tokenService.CreateToken(managedUser, familyMember.Id);

        return new LoginResponse
        {
            Id = managedUser.Id,
            Username = managedUser.UserName!,
            Email = managedUser.Email,
            Token = accessToken,
            Name = familyMember.Name
        };
    }
}