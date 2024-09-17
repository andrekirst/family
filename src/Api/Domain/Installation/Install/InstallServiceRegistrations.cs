namespace Api.Domain.Installation.Install;

public static class InstallServiceRegistrations
{
    public static void AddInstallServices(this IServiceCollection services)
    {
        services.AddScoped<IUserRegistration, UserRegistration>();
    }
}