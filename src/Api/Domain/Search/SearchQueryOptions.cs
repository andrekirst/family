namespace Api.Domain.Search;

public class SearchQueryOptions
{
    public SearchType SearchType { get; set; }
    public int CountItemsReturning { get; set; } = 5;

    public SearchQueryOptions WithSearchType(SearchType searchType)
    {
        SearchType = searchType;
        return this;
    }
}