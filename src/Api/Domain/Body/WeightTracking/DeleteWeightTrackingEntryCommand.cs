using Api.Database;
using Api.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.Domain.Body.WeightTracking;

public record DeleteWeightTrackingEntryCommand(Guid Id, Guid FamilyMemberId) : ICommand;

public class DeleteWeightTrackingEntryCommandValidator : AbstractValidator<DeleteWeightTrackingEntryCommand>
{
    public DeleteWeightTrackingEntryCommandValidator(
        ApplicationDbContext dbContext)
    {
        RuleFor(command => command.FamilyMemberId)
            .MustAsync(dbContext.FamilyMembers.Exists);

        RuleFor(command => command.Id)
            .MustAsync(dbContext.WeightTrackingEntries.Exists);

        RuleFor(command => command)
            .MustAsync((command, token) => dbContext.WeightTrackingEntries.AnyAsync(wte => wte.Id == command.Id, token));
    }
}

public class DeleteWeightTrackingEntryCommandHandler(
    ApplicationDbContext dbContext,
    IMediator mediator) : ICommandHandler<DeleteWeightTrackingEntryCommand>
{
    public async Task Handle(DeleteWeightTrackingEntryCommand request, CancellationToken cancellationToken)
    {
        var entryForDomainEvent = await dbContext.WeightTrackingEntries.AsNoTracking()
            .Where(wte => wte.Id == request.Id)
            .Select(wte => new
            {
                wte.Id,
                wte.MeasuredAt,
                wte.Weight,
                wte.WeightUnit
            })
            .SingleAsync(cancellationToken);

        var deletedRows = await dbContext.WeightTrackingEntries
            .Where(wte => wte.Id == request.Id)
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedRows != 1)
        {
            throw new IncorrectNumberOfWeightTrackingEntriesDeletedException(deletedRows, request.Id, request.FamilyMemberId);
        }

        await mediator.Publish(new WeightTrackingEntryDeletedDomainEvent(entryForDomainEvent.Id, entryForDomainEvent.MeasuredAt, entryForDomainEvent.Weight, entryForDomainEvent.WeightUnit), cancellationToken);
    }
}