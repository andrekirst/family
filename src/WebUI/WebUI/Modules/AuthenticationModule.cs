using Family.Libraries.Extensions.Collections;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

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
                options.SignedOutCallbackPath = section["RedirectUri"] ??  throw new ArgumentException("RedirectUri is missing");
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                options.GetClaimsFromUserInfoEndpoint = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "roles"
                };

                var scopes = section
                    .GetSection("Scopes")
                    .Get<string[]>();

                ArgumentNullException.ThrowIfNull(scopes);
                options.Scope.AddRange(scopes);
        
                options.SaveTokens = true;
            });
    }
}