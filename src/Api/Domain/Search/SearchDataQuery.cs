using System.Runtime.CompilerServices;
using MediatR;

namespace Api.Domain.Search;

public record SearchDataQuery(string Value, SearchType SearchType = SearchType.Default) : IStreamRequest<SearchResult>;

public class SearchDataQueryHandler : IStreamRequestHandler<SearchDataQuery, SearchResult>
{
    private readonly IEnumerable<ISearchDataService> _searchDataServices;
    private readonly ISearchDataQueryOptionsService _searchDataQueryOptionsService;
    private readonly ISearchDataValueParser _searchDataValueParser;

    public SearchDataQueryHandler(
        IEnumerable<ISearchDataService> searchDataServices,
        ISearchDataQueryOptionsService searchDataQueryOptionsService,
        ISearchDataValueParser searchDataValueParser)
    {
        _searchDataServices = searchDataServices;
        _searchDataQueryOptionsService = searchDataQueryOptionsService;
        _searchDataValueParser = searchDataValueParser;
    }

    public async IAsyncEnumerable<SearchResult> Handle(SearchDataQuery request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var searchQueryOptions = await _searchDataQueryOptionsService.GetOptions(cancellationToken);

        if (string.IsNullOrWhiteSpace(request.Value))
        {
            yield break;
        }

        var knownShortcuts = GetKnownShortcuts();

        var (shortcut, value) = _searchDataValueParser.ExtractShortcutAndValue(request.Value, knownShortcuts);

        if (shortcut != null)
        {
            var serviceWithShortcut = _searchDataServices.Single(service => service.Shortcuts.Contains(shortcut));

            var searchResults = serviceWithShortcut.Search(value!, searchQueryOptions, cancellationToken);

            await foreach (var searchResult in searchResults.WithCancellation(cancellationToken))
            {
                yield return new SearchResult
                {
                    ObjectType = searchResult.ObjectType,
                    Title = searchResult.Title,
                    ValueId = searchResult.ValueId
                };
            }
        }

        foreach (var searchDataService in _searchDataServices)
        {
            var searchResults = searchDataService.Search(request.Value, searchQueryOptions, cancellationToken);
            await foreach (var searchResult in searchResults.WithCancellation(cancellationToken))
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
        _searchDataServices
            .SelectMany(service => service.Shortcuts)
            .ToList();
}