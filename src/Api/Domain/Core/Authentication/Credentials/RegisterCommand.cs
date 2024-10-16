using Api.Database;
using Api.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Domain.Core.Authentication.Credentials;

public record RegisterCommand(RegistrationRequest Request) : ICommand<Result<RegisterResponse>>;

public class RegisterResponse;

public class RegisterCommandHandler(
    UsersContext usersContext,
    UserManager<IdentityUser> userManager,
    ISender sender) : ICommandHandler<RegisterCommand, Result<RegisterResponse>>
{
    public async Task<Result<RegisterResponse>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var identityUser = new IdentityUser
        {
            UserName = command.Request.Username,
            Email = command.Request.EMail
        };

        var result = await userManager.CreateAsync(identityUser, command.Request.Password);

        if (!result.Succeeded)
        {
            return Errors.Authentication.IdentityResult(result.Errors);
        }
        
        var userId = await usersContext.Users
            .AsNoTracking()
            .Where(user => user.UserName == command.Request.Username)
            .Select(user => user.Id)
            .SingleAsync(cancellationToken);

        await sender.Send(command.Request.ToCreateFamilyMemberCommand(userId), cancellationToken);

        command.Request.InvalidatePassword();
        return new RegisterResponse();
    }
}