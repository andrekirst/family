using Api.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace Api.Domain.Core.Authentication.Google;

public record GoogleLoginCommand(GoogleLoginRequest Request) : ICommand<Result<LoginResponse>>;

public class GoogleLoginCommandHandler(
    UserManager<IdentityUser> userManager,
    IFamilyMemberRegistrationService familyMemberRegistrationService,
    IFamilyMemberLoginService familyMemberLoginService,
    ITokenService tokenService
    ) : ICommandHandler<GoogleLoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var baseRequest = request.Request;
        
        var managedUser = await userManager.FindByEmailAsync(request.Request.EMail);

        if (managedUser == null)
        {
            var nameParts = request.Request.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            // TODO Refactor when using multiple identity providers
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

            if (!providerRegistration.IsSuccess)
            {
                return providerRegistration.Error!;
            }
            
            var accessToken = tokenService.CreateToken(providerRegistration.Value!.IdentityUser!, providerRegistration.Value.FamilyMemberId!.Value);

            return new LoginResponse
            {
                Email = providerRequest.EMail,
                Name = $"{providerRequest.FirstName} {providerRequest.LastName}",
                Id = providerRegistration.Value.AspNetUserId!,
                Username = providerRequest.Username,
                Token = accessToken
            };
        }

        var providerAccountLoggedIn = await familyMemberLoginService.LoginProviderAccount(new LoginOidcProviderRequest
        {
            EMail = baseRequest.EMail,
            AccessToken = baseRequest.GoogleAccessToken,
            GoogleId = baseRequest.GoogleId
        }, cancellationToken);

        if (providerAccountLoggedIn.IsError)
        {
            return Errors.Authentication.CouldNotLoginToProvider("google");
        }
        
        return new LoginResponse
        {
            Email = baseRequest.EMail,
            Name = providerAccountLoggedIn.Value!.Name!,
            Username = managedUser.UserName!,
            Token = tokenService.CreateToken(managedUser, providerAccountLoggedIn.Value.FamilyMemberId!.Value),
            Id = managedUser.Id
        };
    }
}