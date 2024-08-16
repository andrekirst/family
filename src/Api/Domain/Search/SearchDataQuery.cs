using System.Runtime.CompilerServices;
using MediatR;

namespace Api.Domain.Search;

public record SearchDataQuery(string Value, SearchType SearchType = SearchType.Default) : IStreamRequest<SearchResult>;

public class SearchDataQueryHandler(
    IEnumerable<ISearchDataService> searchDataServices,
    ISearchDataQueryOptionsService searchDataQueryOptionsService,
    ISearchDataValueParser searchDataValueParser)
    : IStreamRequestHandler<SearchDataQuery, SearchResult>
{
    public async IAsyncEnumerable<SearchResult> Handle(SearchDataQuery request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var searchQueryOptions = await searchDataQueryOptionsService.GetOptions(cancellationToken);

        if (string.IsNullOrWhiteSpace(request.Value))
        {
            yield break;
        }

        var knownShortcuts = GetKnownShortcuts();

        var (shortcut, value) = searchDataValueParser.ExtractShortcutAndValue(request.Value, knownShortcuts);

        if (shortcut != null)
        {
            var serviceWithShortcut = searchDataServices.Single(service => service.Shortcuts.Contains(shortcut));

            var searchResults = serviceWithShortcut.Search(value!, searchQueryOptions, cancellationToken);

            await foreach (var searchResult in searchResults)
            {
                yield return new SearchResult
                {
                    ObjectType = searchResult.ObjectType,
                    Title = searchResult.Title,
                    ValueId = searchResult.ValueId
                };
            }
        }

        foreach (var searchDataService in searchDataServices)
        {
            var searchResults = searchDataService.Search(request.Value, searchQueryOptions, cancellationToken);
            await foreach (var searchResult in searchResults)
            {
                yield return new SearchResult
                {
                    ObjectType = searchResult.ObjectType,
                    Title = searchResult.Title,
                    ValueId = searchResult.ValueId
                };
            }
        }
    }

    private List<string> GetKnownShortcuts() =>
        searchDataServices
            .SelectMany(service => service.Shortcuts)
            .ToList();
}