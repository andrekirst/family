using System.Runtime.CompilerServices;
using Api.Database;
using Microsoft.EntityFrameworkCore;

namespace Api.Domain.Search;

public class SearchFamilyMemberDataService(ApplicationDbContext dbContext) : ISearchDataService
{
    public async IAsyncEnumerable<SearchDataResult> Search(string value, SearchQueryOptions options, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var countItemsReturning = options.CountItemsReturning;
        var itemsReturned = 0;
        var idsAlreadyReturned = new List<Guid>();
        var items = Rank1Query(value, countItemsReturning);

        await foreach (var item in items.WithCancellation(cancellationToken))
        {
            itemsReturned++;
            idsAlreadyReturned.Add(item.ValueId);
            yield return item;
        }

        if (itemsReturned == countItemsReturning)
        {
            yield break;
        }

        items = Rank2Query(value, idsAlreadyReturned, countItemsReturning, itemsReturned);

        await foreach (var item in items.WithCancellation(cancellationToken))
        {
            itemsReturned++;
            idsAlreadyReturned.Add(item.ValueId);
            yield return item;
        }
    }

    private IAsyncEnumerable<SearchDataResult> Rank2Query(string value, List<Guid> idsAlreadyReturned, int countItemsReturning, int itemsReturned)
    {
        return dbContext.FamilyMembers
            .Where(fm => fm.FirstName != null && fm.FirstName.Contains(value) ||
                         fm.LastName != null && fm.LastName.Contains(value))
            .Where(fm => !idsAlreadyReturned.Contains(fm.Id))
            .OrderBy(fm => fm.FirstName)
            .ThenBy(fm => fm.LastName)
            .Take(countItemsReturning - itemsReturned)
            .Select(fm => new SearchDataResult
            {
                ObjectType = ObjectType.FamilyMember,
                Title = fm.FirstName + " " + fm.LastName,
                ValueId = fm.Id
            })
            .AsAsyncEnumerable();
    }

    private IAsyncEnumerable<SearchDataResult> Rank1Query(string value, int countItemsReturning)
    {
        return dbContext.FamilyMembers
            .Where(fm => fm.FirstName != null && fm.FirstName.StartsWith(value) ||
                         fm.LastName != null && fm.LastName.StartsWith(value))
            .OrderBy(fm => fm.FirstName)
            .ThenBy(fm => fm.LastName)
            .Take(countItemsReturning)
            .Select(fm => new SearchDataResult
            {
                ObjectType = ObjectType.FamilyMember,
                Title = fm.FirstName + " " + fm.LastName,
                ValueId = fm.Id
            })
            .AsAsyncEnumerable();
    }

    public string[] Shortcuts => ["fm", "fam"];
}