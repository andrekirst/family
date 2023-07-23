namespace Api.Domain.Search;

public interface ISearchDataQueryOptionsService
{
    Task<SearchQueryOptions> GetOptions(CancellationToken cancellationToken = default);
}

public class SearchDataQueryOptionsService : ISearchDataQueryOptionsService
{
    public async Task<SearchQueryOptions> GetOptions(CancellationToken cancellationToken = default)
    {
        return new SearchQueryOptions
        {
            SearchType = SearchType.Quick
        };
    }
}