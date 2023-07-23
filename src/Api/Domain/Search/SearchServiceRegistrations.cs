namespace Api.Domain.Search;

public static class SearchServiceRegistrations
{
    public static void AddSearchServices(this IServiceCollection services)
    {
        services.AddScoped<ISearchDataService, SearchFamilyMemberDataService>();
        services.AddScoped<ISearchDataQueryOptionsService, SearchDataQueryOptionsService>();
        services.AddScoped<ISearchDataValueParser, SearchDataValueParser>();
    }
}