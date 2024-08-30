namespace Api.Infrastructure;

public class GoogleAuthenticationOptions
{
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string RedirectUri { get; set; } = null!;
}

public static class GoogleAuthenticationOptionsRegistration
{
    public static void UseGoogleAuthenticationOptions(this IServiceCollection services)
    {
        services
            .AddOptions<GoogleAuthenticationOptions>()
            .BindConfiguration("Authentication:Google");
    }
}