using Api.Database;
using Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Calendar.Me;

public record GetCalendarMeListQuery : IQuery<Result<GetCalendarMeListQueryResponse>>;

public class ListQueryHandler(
    ApplicationDbContext dbContext,
    IHttpContextAccessor httpContextAccessor)
    : IQueryHandler<GetCalendarMeListQuery, Result<GetCalendarMeListQueryResponse>>
{
    public async Task<Result<GetCalendarMeListQueryResponse>> Handle(GetCalendarMeListQuery request, CancellationToken cancellationToken)
    {
        var getFamilyMemberIdResult = httpContextAccessor.HttpContext?.GetFamilyMemberId()!;
        if (getFamilyMemberIdResult.IsError)
        {
            return getFamilyMemberIdResult.Error!;
        }

        var familyMemberId = getFamilyMemberIdResult.Value;

        var query = await dbContext.Calendars.AsNoTracking()
            .Where(c => c.FamilyMemberId == familyMemberId)
            .Select(c => new GetCalendarMeListItemQueryResponse
            {
                Id = c.Id,
                Name = c.Name!
            })
            .ToListAsync(cancellationToken);

        return new GetCalendarMeListQueryResponse
        {
            Count = query.Count,
            Items = query
        };
    }
}

public class GetCalendarMeListQueryResponse
{
    public int Count { get; set; }
    public List<GetCalendarMeListItemQueryResponse> Items { get; set; } = new();
}

public class GetCalendarMeListItemQueryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
}