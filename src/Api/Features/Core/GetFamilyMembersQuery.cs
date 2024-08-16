using Api.Database;
using Api.Domain.Core;
using Api.Infrastructure;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Core;

public record GetFamilyMembersQuery : IQuery<List<GetFamilyMembersQueryDto>>;

public class GetFamilyMembersQueryDto
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? BirthDate { get; set; }
}

public class GetFamilyMembersQueryHandler(
    ApplicationDbContext dbContext,
    IMapper mapper) : IQueryHandler<GetFamilyMembersQuery, List<GetFamilyMembersQueryDto>>
{
    public Task<List<GetFamilyMembersQueryDto>> Handle(GetFamilyMembersQuery request, CancellationToken cancellationToken) =>
        dbContext.FamilyMembers
            .ProjectTo<GetFamilyMembersQueryDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
}

public class GetFamilyMembersQueryMappings : Profile
{
    public GetFamilyMembersQueryMappings()
    {
        CreateMap<FamilyMember, GetFamilyMembersQueryDto>();
    }
}
