namespace Api.Domain.Core.Authentication;

public static class ServiceRegistrations
{
    public static void AddAuthenticationServices(this IServiceCollection services)
    {
        services.AddScoped<IFamilyMemberLoginService, FamilyMemberLoginService>();
        services.AddScoped<IFamilyMemberRegistrationService, FamilyMemberRegistrationService>();
    }
}