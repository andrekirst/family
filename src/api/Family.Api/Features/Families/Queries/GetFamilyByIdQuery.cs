using Family.Api.Features.Families.DTOs;
using Family.Infrastructure.CQRS.Abstractions;
using MediatR;

namespace Family.Api.Features.Families.Queries;

public record GetFamilyByIdQuery(string FamilyId) : IQuery<FamilyDto?>;

public class GetFamilyByIdQueryHandler : IRequestHandler<GetFamilyByIdQuery, FamilyDto?>
{
    private readonly IFamilyRepository _familyRepository;

    public GetFamilyByIdQueryHandler(IFamilyRepository familyRepository)
    {
        _familyRepository = familyRepository;
    }

    public async Task<FamilyDto?> Handle(GetFamilyByIdQuery request, CancellationToken cancellationToken)
    {
        var family = await _familyRepository.GetByIdAsync(request.FamilyId, cancellationToken);
        return family != null ? FamilyDto.FromDomain(family) : null;
    }
}