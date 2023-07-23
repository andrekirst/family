namespace Api.Domain.Search;

public interface ISearchDataService
{
    IAsyncEnumerable<SearchDataResult> Search(string value, SearchQueryOptions options, CancellationToken cancellationToken = default);

    string[] Shortcuts { get; }
}