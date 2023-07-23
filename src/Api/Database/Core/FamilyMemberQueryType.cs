using Api.Domain.Core;
using Microsoft.EntityFrameworkCore;

namespace Api.Database.Core;

[ExtendObjectType("QueryType")]
public class FamilyMemberQueryType
{
    [UseProjection]
    [UseSorting]
    [UseFiltering]
    public IQueryable<FamilyMember> GetFamilyMembers([Service] ApplicationDbContext dbContext) =>
        dbContext.FamilyMembers.TagWithCallSite();
}