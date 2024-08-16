using Api.Database;
using Api.Domain.Core;
using Api.Infrastructure;
using FluentValidation;
using MediatR;

namespace Api.Domain.Body.WeightTracking;

public record CreateWeightTrackingEntryCommand(Guid FamilyMemberId, CreateWeightTrackingEntryCommandModel Model) : ICommand;

public record CreateWeightTrackingEntryCommandModel(DateTime MeasuredAt, WeightUnit WeightUnit, double Weight);

public class CreateWeightTrackingEntryCommandValidator : AbstractValidator<CreateWeightTrackingEntryCommand>
{
    public CreateWeightTrackingEntryCommandValidator(ApplicationDbContext dbContext)
    {
        RuleFor(command => Weight.FromRaw(command.Model.Weight))
            .SetValidator(new WeightValidator())
            .OverridePropertyName(nameof(CreateWeightTrackingEntryCommand.Model.Weight));

        RuleFor(command => command.FamilyMemberId)
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
        var familyMemberId = currentFamilyMemberIdService.GetFamilyMemberId();
        var weightTrackingEntry = WeightTrackingEntryMappings.MapFromSource(request.Model);

        dbContext.WeightTrackingEntries.Add(weightTrackingEntry);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        var domainEvent = CreateDomainEvent(request, weightTrackingEntry, familyMemberId);
        await mediator.Publish(domainEvent, cancellationToken);
    }

    private static WeightTrackingEntryCreatedDomainEvent CreateDomainEvent(CreateWeightTrackingEntryCommand request, WeightTrackingEntry weightTrackingEntry, Guid familyMemberId)
    {
        return new WeightTrackingEntryCreatedDomainEvent(
            weightTrackingEntry.Id,
            weightTrackingEntry.MeasuredAt,
            weightTrackingEntry.WeightUnit,
            weightTrackingEntry.Weight,
            familyMemberId,
            request.FamilyMemberId);
    }
}
