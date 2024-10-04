using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace WebUI.Modules;

public static class AuthenticationModule
{
    public const string ZitadelAuthenticationSchema = "Zitadel";

    public static void AddAppAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect(ZitadelAuthenticationSchema, options =>
            {
                var section = builder.Configuration.GetSection($"Authentication:{ZitadelAuthenticationSchema}");
                options.Authority = section["Authority"];
                options.ClientId = section["ClientId"];
                options.CallbackPath = section["CallbackPath"];
                options.SignedOutCallbackPath = section["RedirectUri"] ??  "/account/signedout";
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.RequireHttpsMetadata = false;
                options.GetClaimsFromUserInfoEndpoint = true;

                // Legt fest, welche Claims und Token zu laden sind
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.Scope.Add("offline_access");
                options.Scope.AddRange("family_name", "gender", "given_name");
        
                options.SaveTokens = true;
            });
    }
}