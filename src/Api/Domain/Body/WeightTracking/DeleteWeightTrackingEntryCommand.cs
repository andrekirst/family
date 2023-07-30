using Api.Database;
using Api.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.Domain.Body.WeightTracking;

public record DeleteWeightTrackingEntryCommand(int Id, int FamilyMemberId) : ICommand;

public class DeleteWeightTrackingEntryCommandValidator : AbstractValidator<DeleteWeightTrackingEntryCommand>
{
    public DeleteWeightTrackingEntryCommandValidator(
        ApplicationDbContext dbContext)
    {
        RuleFor(_ => _.FamilyMemberId)
            .MustAsync(dbContext.FamilyMembers.Exists);

        RuleFor(_ => _.Id)
            .MustAsync(dbContext.WeightTrackingEntries.Exists);

        RuleFor(_ => _)
            .MustAsync((command, token) => dbContext.WeightTrackingEntries.AnyAsync(wte => wte.Id == command.Id, token));
    }
}

public class DeleteWeightTrackingEntryCommandHandler : ICommandHandler<DeleteWeightTrackingEntryCommand>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMediator _mediator;

    public DeleteWeightTrackingEntryCommandHandler(
        ApplicationDbContext dbContext,
        IMediator mediator)
    {
        _dbContext = dbContext;
        _mediator = mediator;
    }

    public async Task Handle(DeleteWeightTrackingEntryCommand request, CancellationToken cancellationToken)
    {
        var entryForDomainEvent = await _dbContext.WeightTrackingEntries.AsNoTracking()
            .Where(wte => wte.Id == request.Id)
            .Select(wte => new
            {
                wte.Id,
                wte.MeasuredAt,
                wte.Weight,
                wte.WeightUnit
            })
            .SingleAsync(cancellationToken);

        var deletedRows = await _dbContext.WeightTrackingEntries
            .Where(wte => wte.Id == request.Id)
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedRows != 1)
        {
            throw new IncorrectNumberOfWeightTrackingEntriesDeletedException(deletedRows, request.Id, request.FamilyMemberId);
        }

        await _mediator.Publish(new WeightTrackingEntryDeletedDomainEvent(entryForDomainEvent.Id, entryForDomainEvent.MeasuredAt, entryForDomainEvent.Weight, entryForDomainEvent.WeightUnit), cancellationToken);
    }
}