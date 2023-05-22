using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebUi.Data;

namespace WebUi.Areas.Administration.FamilyMembers.Detail;

public record Get(int Id) : IRequest<ViewModel>;

public class GetHandler : IRequestHandler<Get, ViewModel>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetHandler(
        ApplicationDbContext dbContext,
        IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }
    
    public async Task<ViewModel> Handle(Get request, CancellationToken cancellationToken)
    {
        var viewModel = await _dbContext.FamilyMembers
            .Where(familyMember => familyMember.Id == request.Id)
            .ProjectTo<ViewModel>(_mapper.ConfigurationProvider)
            .SingleAsync(cancellationToken);

        return viewModel;
    }
}