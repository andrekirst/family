using Api.Database;
using Api.Database.Body;
using Api.Domain.Body;
using Api.Infrastructure;
using Api.Infrastructure.Database;
using FluentValidation;
using MediatR;

namespace Api.Features.Body;

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
        IHttpContextAccessor httpContextAccessor)
    : ICommandHandler<CreateWeightTrackingEntryCommand>
{
    public async Task Handle(CreateWeightTrackingEntryCommand request, CancellationToken cancellationToken)
    {
        var familyMemberId = httpContextAccessor.HttpContext?.GetFamilyMemberId();
        
        var weightTrackingEntry = WeightTrackingEntryMappings.MapFromSource(request.Model);

        dbContext.WeightTrackings.Add(weightTrackingEntry);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        var domainEvent = CreateDomainEvent(request, weightTrackingEntry, familyMemberId!.Value);
        await mediator.Publish(domainEvent, cancellationToken);
    }

    private static WeightTrackingEntryCreatedDomainEvent CreateDomainEvent(CreateWeightTrackingEntryCommand request, WeightTrackingEntity weightTrackingEntity, Guid familyMemberId)
    {
        return new WeightTrackingEntryCreatedDomainEvent(
            weightTrackingEntity.Id,
            weightTrackingEntity.MeasuredAt,
            weightTrackingEntity.WeightUnit,
            weightTrackingEntity.Weight,
            familyMemberId,
            request.FamilyMemberId);
    }
}
