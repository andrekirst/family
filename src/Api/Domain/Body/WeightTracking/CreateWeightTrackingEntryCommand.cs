using Api.Database;
using Api.Domain.Core;
using Api.Infrastructure;
using FluentValidation;
using MediatR;

namespace Api.Domain.Body.WeightTracking;

public record CreateWeightTrackingEntryCommand(int FamilyMemberId, CreateWeightTrackingEntryCommandModel Model) : ICommand;

public record CreateWeightTrackingEntryCommandModel(DateTime MeasuredAt, WeightUnit WeightUnit, double Weight);

public class CreateWeightTrackingEntryCommandValidator : AbstractValidator<CreateWeightTrackingEntryCommand>
{
    public CreateWeightTrackingEntryCommandValidator(ApplicationDbContext dbContext)
    {
        RuleFor(_ => Weight.FromRaw(_.Model.Weight))
            .SetValidator(new WeightValidator())
            .OverridePropertyName(nameof(CreateWeightTrackingEntryCommand.Model.Weight));

        RuleFor(_ => _.FamilyMemberId)
            .MustAsync(dbContext.FamilyMembers.Exists);
    }
}

public class CreateWeightTrackingEntryCommandHandler(
        ApplicationDbContext dbContext,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        CurrentFamilyMemberIdService currentFamilyMemberIdService)
    : ICommandHandler<CreateWeightTrackingEntryCommand>
{
    public async Task Handle(CreateWeightTrackingEntryCommand request, CancellationToken cancellationToken)
    {
        var createdByFamilyMemberId = currentFamilyMemberIdService.GetFamilyMemberId();
        var weightTrackingEntry = WeightTrackingEntryMappings.MapTo(request.Model);

        dbContext.WeightTrackingEntries.Add(weightTrackingEntry);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        var domainEvent = new WeightTrackingEntryCreatedDomainEvent(
            weightTrackingEntry.Id,
            weightTrackingEntry.MeasuredAt,
            weightTrackingEntry.WeightUnit,
            weightTrackingEntry.Weight,
            createdByFamilyMemberId,
            request.FamilyMemberId);
        await mediator.Publish(domainEvent, cancellationToken);
    }
}
