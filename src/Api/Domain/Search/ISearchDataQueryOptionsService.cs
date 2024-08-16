namespace Api.Domain.Search;

public interface ISearchDataQueryOptionsService
{
    Task<SearchQueryOptions> GetOptions(CancellationToken cancellationToken = default);
}

public class SearchDataQueryOptionsService : ISearchDataQueryOptionsService
{
    public Task<SearchQueryOptions> GetOptions(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SearchQueryOptions
        {
            SearchType = SearchType.Quick
        });
    }
}