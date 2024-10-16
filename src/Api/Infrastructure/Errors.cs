using Microsoft.AspNetCore.Identity;

namespace Api.Infrastructure;

public static class Errors
{
    public static class Authentication
    {
        public static Error BadCredentials() => new Error("Authentication:BadCredentials");
        public static Error CouldNotLoginToProvider(string providerName) => new Error("Authentication:CouldNotLoginToProvider", $"Could not login to provider {providerName}");

        public static Error IdentityResult(IEnumerable<IdentityError> identityErrors)
        {
            var firstIdentityError = identityErrors.FirstOrDefault();
            return firstIdentityError == null
                ? new Error("Authentication:IdentityResult", "unknwon error")
                : new Error($"Authentication:IdentityResult:{firstIdentityError.Code}", firstIdentityError.Description);
        }
    }
}