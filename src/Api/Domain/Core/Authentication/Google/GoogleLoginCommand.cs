using Api.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace Api.Domain.Core.Authentication.Google;

public record GoogleLoginCommand(GoogleLoginRequest Request) : ICommand<bool>;

public class GoogleLoginCommandHandler(
    UserManager<IdentityUser> userManager,
    IFamilyMemberRegistrationService familyMemberRegistrationService,
    IFamilyMemberLoginService familyMemberLoginService
    ) : ICommandHandler<GoogleLoginCommand, bool>
{
    public async Task<bool> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var managedUser = await userManager.FindByEmailAsync(request.Request.EMail);

        if (managedUser == null)
        {
            var nameParts = request.Request.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var registerOidcProviderRequest = new RegisterOidcProviderRequest
            {
                Birthdate = DateTime.Today,
                FirstName = request.Request.FirstName ?? nameParts.FirstOrDefault() ?? "<Unknwon FirstName>",
                LastName = request.Request.LastName ?? nameParts.LastOrDefault() ?? "<Unknwon LastName>",
                Username = request.Request.EMail,
                EMail = request.Request.EMail,
                AccessToken = request.Request.AccessToken,
                GoogleId = request.Request.GoogleId,
                // TODO Refactor when using more identity provicers
                ProviderName = "google"
            };
            
            return await familyMemberRegistrationService.RegisterProviderAccount(registerOidcProviderRequest, cancellationToken);
        }

        return await familyMemberLoginService.LoginProviderAccount(new LoginOidcProviderRequest
        {
            EMail = request.Request.EMail,
            AccessToken = request.Request.AccessToken!,
            GoogleId = request.Request.GoogleId
        }, cancellationToken);
    }
}