using Api.Domain.Body.WeightTracking;
using Microsoft.EntityFrameworkCore;

namespace Api.Database.Body;

[ExtendObjectType("QueryType")]
public class WeightTrackingQueryType
{
    [UseProjection]
    [UseSorting]
    [UseFiltering]
    public IQueryable<WeightTrackingEntry> GetWeightTrackingEntries([Service] ApplicationDbContext dbContext) =>
        dbContext.WeightTrackingEntries.TagWithCallSite();
}