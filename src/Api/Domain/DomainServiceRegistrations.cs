using Api.Domain.Core.Authentication;
using Api.Domain.Installation.Install;
using Api.Domain.Search;

namespace Api.Domain;

public static class DomainServiceRegistrations
{
    public static void AddDomainServices(this IServiceCollection services)
    {
        services.AddSearchServices();
        services.AddInstallServices();
        services.AddAuthenticationServices();
    }
}