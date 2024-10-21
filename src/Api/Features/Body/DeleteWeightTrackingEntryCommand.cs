using Api.Database;
using Api.Domain.Body;
using Api.Infrastructure;
using Api.Infrastructure.Database;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Body;

public record DeleteWeightTrackingEntryCommand(Guid Id, Guid FamilyMemberId) : ICommand;

public class DeleteWeightTrackingEntryCommandValidator : AbstractValidator<DeleteWeightTrackingEntryCommand>
{
    public DeleteWeightTrackingEntryCommandValidator(
        ApplicationDbContext dbContext)
    {
        RuleFor(command => command.FamilyMemberId)
            .MustAsync(dbContext.FamilyMembers.Exists);

        RuleFor(command => command.Id)
            .MustAsync(dbContext.WeightTrackings.Exists);

        RuleFor(command => command)
            .MustAsync((command, token) => dbContext.WeightTrackings.AnyAsync(wte => wte.Id == command.Id, token));
    }
}

public class DeleteWeightTrackingEntryCommandHandler(
    ApplicationDbContext dbContext,
    IMediator mediator) : ICommandHandler<DeleteWeightTrackingEntryCommand>
{
    public async Task Handle(DeleteWeightTrackingEntryCommand request, CancellationToken cancellationToken)
    {
        var entryForDomainEvent = await dbContext.WeightTrackings.AsNoTracking()
            .Where(wte => wte.Id == request.Id)
            .Select(wte => new
            {
                wte.Id,
                wte.MeasuredAt,
                wte.Weight,
                wte.WeightUnit
            })
            .SingleAsync(cancellationToken);

        var deletedRows = await dbContext.WeightTrackings
            .Where(wte => wte.Id == request.Id)
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedRows != 1)
        {
            throw new IncorrectNumberOfWeightTrackingEntriesDeletedException(deletedRows, request.Id, request.FamilyMemberId);
        }

        await mediator.Publish(new WeightTrackingEntryDeletedDomainEvent(entryForDomainEvent.Id, entryForDomainEvent.MeasuredAt, entryForDomainEvent.Weight, entryForDomainEvent.WeightUnit), cancellationToken);
    }
}