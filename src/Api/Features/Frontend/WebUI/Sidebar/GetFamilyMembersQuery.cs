using Api.Database;
using Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Frontend.WebUI.Sidebar;

public class GetFamilyMembersQuery : IQuery<List<Response>>;

public class GetFamilyMembersQueryHandler(ApplicationDbContext dbContext) : IQueryHandler<GetFamilyMembersQuery, List<Response>>
{
    public async Task<List<Response>> Handle(GetFamilyMembersQuery request, CancellationToken cancellationToken)
    {
        var query = from familymember in dbContext.FamilyMembers
            select new Response
            {
                Id = familymember.Id,
                DisplayText = $"{familymember.FirstName} {familymember.LastName}"
            };

        return await query.ToListAsync(cancellationToken);
    }
}

public class Response
{
    public Guid Id { get; set; }
    public string DisplayText { get; set; } = default!;
}