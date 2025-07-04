using Family.Api.Features.Families.DTOs;
using Family.Infrastructure.CQRS.Abstractions;
using MediatR;

namespace Family.Api.Features.Families.Queries;

public record GetUserFamilyQuery(Guid UserId) : IQuery<FamilyDto?>;

public class GetUserFamilyQueryHandler : IRequestHandler<GetUserFamilyQuery, FamilyDto?>
{
    private readonly IFamilyRepository _familyRepository;

    public GetUserFamilyQueryHandler(IFamilyRepository familyRepository)
    {
        _familyRepository = familyRepository;
    }

    public async Task<FamilyDto?> Handle(GetUserFamilyQuery request, CancellationToken cancellationToken)
    {
        var family = await _familyRepository.GetByMemberIdAsync(request.UserId, cancellationToken);
        return family != null ? FamilyDto.FromDomain(family) : null;
    }
}