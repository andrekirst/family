using Api.Database;
using Api.Domain.Core;
using Api.Infrastructure;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Api.Domain.Body.WeightTracking;

public record CreateWeightTrackingEntryCommand(CreateWeightTrackingEntryCommandModel Model) : ICommand;

public record CreateWeightTrackingEntryCommandModel(int FamilyMemberId, DateTime MeasuredAt, WeightUnit WeightUnit, double Weight);

public class CreateWeightTrackingEntryCommandValidator : AbstractValidator<CreateWeightTrackingEntryCommand>
{
    public CreateWeightTrackingEntryCommandValidator(ApplicationDbContext dbContext)
    {
        RuleFor(_ => Weight.FromRaw(_.Model.Weight))
            .SetValidator(new WeightValidator())
            .OverridePropertyName(nameof(CreateWeightTrackingEntryCommand.Model.Weight));

        RuleFor(_ => _.Model.FamilyMemberId)
            .MustAsync(dbContext.FamilyMembers.Exists);
    }
}

public class CreateWeightTrackingEntryCommandHandler : ICommandHandler<CreateWeightTrackingEntryCommand>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly CurrentFamilyMemberIdService _currentFamilyMemberIdService;

    public CreateWeightTrackingEntryCommandHandler(
        ApplicationDbContext dbContext,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        IMapper mapper,
        CurrentFamilyMemberIdService currentFamilyMemberIdService)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _mapper = mapper;
        _currentFamilyMemberIdService = currentFamilyMemberIdService;
    }

    public async Task Handle(CreateWeightTrackingEntryCommand request, CancellationToken cancellationToken)
    {
        var createdByFamilyMemberId = _currentFamilyMemberIdService.GetFamilyMemberId();
        var weightTrackingEntry = _mapper.Map<WeightTrackingEntry>(request.Model);
        _dbContext.WeightTrackingEntries.Add(weightTrackingEntry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        var domainEvent = new WeightTrackingEntryCreatedDomainEvent(
            weightTrackingEntry.Id,
            weightTrackingEntry.MeasuredAt,
            weightTrackingEntry.WeightUnit,
            weightTrackingEntry.Weight,
            createdByFamilyMemberId,
            request.Model.FamilyMemberId);
        await _mediator.Publish(domainEvent, cancellationToken);
    }
}

public class CreateWeightTrackingEntryCommandMappings : Profile
{
    public CreateWeightTrackingEntryCommandMappings()
    {
        CreateMap<CreateWeightTrackingEntryCommandModel, WeightTrackingEntry>();
    }
}