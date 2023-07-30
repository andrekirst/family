using Api.Database;
using Api.Infrastructure;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.Domain.Body.WeightTracking;

public record UpdateWeightTrackingEntryCommand(int Id, int FamilyMemberId, UpdateWeightTrackingEntryCommandModel Model) : ICommand;

public record UpdateWeightTrackingEntryCommandModel(DateTime MeasuredAt, WeightUnit WeightUnit, double Weight);

public class UpdateWeightTrackingEntryCommandValidator : AbstractValidator<UpdateWeightTrackingEntryCommand>
{
    public UpdateWeightTrackingEntryCommandValidator(ApplicationDbContext dbContext)
    {
        // TODO FamilyMemberId-Check
        RuleFor(_ => Weight.FromRaw(_.Model.Weight))
            .SetValidator(new WeightValidator())
            .OverridePropertyName(nameof(CreateWeightTrackingEntryCommand.Model.Weight));

        RuleFor(_ => _.Id)
            .MustAsync(dbContext.WeightTrackingEntries.Exists);
    }
}

public class UpdateWeightTrackingEntryCommandHandler : ICommandHandler<UpdateWeightTrackingEntryCommand>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public UpdateWeightTrackingEntryCommandHandler(
        ApplicationDbContext dbContext,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        IMapper mapper)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _mapper = mapper;
    }

    public async Task Handle(UpdateWeightTrackingEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = await _dbContext.WeightTrackingEntries
            .Where(wte => wte.Id == request.Id)
            .SingleAsync(cancellationToken);

        _mapper.Map(request.Model, entry);

        _dbContext.Update(entry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(new WeightTrackingEntryUpdatedDomainEvent(request.Id, request.Model.MeasuredAt, request.Model.Weight, request.Model.WeightUnit), cancellationToken);
    }
}

public class UpdateWeightTrackingEntryCommandMappings : Profile
{
    public UpdateWeightTrackingEntryCommandMappings()
    {
        CreateMap<UpdateWeightTrackingEntryCommandModel, WeightTrackingEntry>();
    }
}