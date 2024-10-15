using Api.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace Api.Domain.Core.Authentication.Google;

public record GoogleLoginCommand(GoogleLoginRequest Request) : ICommand<LoginResponse>;

public class GoogleLoginCommandHandler(
    UserManager<IdentityUser> userManager,
    IFamilyMemberRegistrationService familyMemberRegistrationService,
    IFamilyMemberLoginService familyMemberLoginService,
    ITokenService tokenService
    ) : ICommandHandler<GoogleLoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var baseRequest = request.Request;
        
        var managedUser = await userManager.FindByEmailAsync(request.Request.EMail);

        if (managedUser == null)
        {
            var nameParts = request.Request.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var providerRequest = new RegisterOidcProviderRequest
            {
                Birthdate = DateTime.Today,
                FirstName = baseRequest.FirstName ?? nameParts.FirstOrDefault() ?? "<Unknwon FirstName>",
                LastName = baseRequest.LastName ?? nameParts.LastOrDefault() ?? "<Unknwon LastName>",
                Username = baseRequest.EMail,
                EMail = baseRequest.EMail,
                GoogleAccessToken = baseRequest.GoogleAccessToken,
                GoogleId = baseRequest.GoogleId,
                ProviderName = "google"
            };
            
            var providerRegistration = await familyMemberRegistrationService.RegisterProviderAccount(providerRequest, cancellationToken);
            var accessToken = tokenService.CreateToken(providerRegistration.IdentityUser!, providerRegistration.FamilyMemberId!.Value);

            return providerRegistration.IsSuccess
                ? new LoginResponse
                {
                    Email = providerRequest.EMail,
                    Name = $"{providerRequest.FirstName} {providerRequest.LastName}",
                    Id = providerRegistration.AspNetUserId!,
                    Username = providerRequest.Username,
                    Token = accessToken
                }
                : throw new GoogleLoginFailedException();
        }

        var providerAccountLoggedIn = await familyMemberLoginService.LoginProviderAccount(new LoginOidcProviderRequest
        {
            EMail = baseRequest.EMail,
            AccessToken = baseRequest.GoogleAccessToken,
            GoogleId = baseRequest.GoogleId
        }, cancellationToken);

        if (providerAccountLoggedIn.IsError) throw new GoogleLoginFailedException();
        
        return new LoginResponse
        {
            Email = baseRequest.EMail,
            Name = providerAccountLoggedIn.Name!,
            Username = managedUser.UserName!,
            Token = tokenService.CreateToken(managedUser, providerAccountLoggedIn.FamilyMemberId!.Value),
            Id = managedUser.Id
        };
    }
}

public class GoogleLoginFailedException : Exception
{
}