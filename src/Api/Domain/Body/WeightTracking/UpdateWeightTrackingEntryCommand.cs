using Api.Database;
using Api.Infrastructure;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.Domain.Body.WeightTracking;

public record UpdateWeightTrackingEntryCommand(Guid Id, Guid FamilyMemberId, UpdateWeightTrackingEntryCommandModel Model) : ICommand;

public record UpdateWeightTrackingEntryCommandModel(DateTime MeasuredAt, WeightUnit WeightUnit, double Weight);

public class UpdateWeightTrackingEntryCommandValidator : AbstractValidator<UpdateWeightTrackingEntryCommand>
{
    public UpdateWeightTrackingEntryCommandValidator(ApplicationDbContext dbContext)
    {
        // TODO FamilyMemberId-Check
        RuleFor(command => Weight.FromRaw(command.Model.Weight))
            .SetValidator(new WeightValidator())
            .OverridePropertyName(nameof(CreateWeightTrackingEntryCommand.Model.Weight));

        RuleFor(command => command.Id)
            .MustAsync(dbContext.WeightTrackingEntries.Exists);
    }
}

public class UpdateWeightTrackingEntryCommandHandler(
    ApplicationDbContext dbContext,
    IUnitOfWork unitOfWork,
    IMediator mediator,
    IMapper mapper)
    : ICommandHandler<UpdateWeightTrackingEntryCommand>
{
    public async Task Handle(UpdateWeightTrackingEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = await dbContext.WeightTrackingEntries
            .Where(wte => wte.Id == request.Id)
            .SingleAsync(cancellationToken);

        mapper.Map(request.Model, entry);

        dbContext.Update(entry);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await mediator.Publish(new WeightTrackingEntryUpdatedDomainEvent(request.Id, request.Model.MeasuredAt, request.Model.Weight, request.Model.WeightUnit), cancellationToken);
    }
}

public class UpdateWeightTrackingEntryCommandMappings : Profile
{
    public UpdateWeightTrackingEntryCommandMappings()
    {
        CreateMap<UpdateWeightTrackingEntryCommandModel, WeightTrackingEntry>();
    }
}