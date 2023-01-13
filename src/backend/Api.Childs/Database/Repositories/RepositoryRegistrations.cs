namespace Api.Childs.Database.Repositories;

public static class RepositoryRegistrations
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IChildRepository, ChildRepository>();
        return services;
    }
}