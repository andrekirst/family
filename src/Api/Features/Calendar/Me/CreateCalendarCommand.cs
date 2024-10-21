using Api.Database;
using Api.Database.Calendar;
using Api.Domain;
using Api.Domain.Calendar;
using Api.Infrastructure;
using Api.Infrastructure.DomainEvents;
using FluentValidation;

namespace Api.Features.Calendar.Me;

public record CreateCalendarCommand(CreateCalendarRequest Request) : ICommand<Result<CreateCalendarResponse>>;

public class CreateCalendarRequest
{
    public string Name { get; set; } = default!;
}

public class CreateCalendarCommandValidator : AbstractValidator<CreateCalendarCommand>
{
    public CreateCalendarCommandValidator()
    {
        RuleFor(c => c.Request.Name)
            .NotNull()
            .NotEmpty()
            .MaximumLength(DefaultLengths.Text);
    }
}

public class CreateCalendarCommandHandler(
    IDomainEventBus domainEventBus,
    ApplicationDbContext dbContext,
    IUnitOfWork unitOfWork,
    IHttpContextAccessor httpContextAccessor) : CommandHandler<CreateCalendarCommand, Result<CreateCalendarResponse>>(httpContextAccessor, unitOfWork, domainEventBus)
{
    public override async Task<Result<CreateCalendarResponse>> Handle(CreateCalendarCommand command, CancellationToken cancellationToken)
    {
        var familyMemberId = FamilyMemberId;
        var entity = new CalendarEntity
        {
            Name = command.Request.Name,
            FamilyMemberId = familyMemberId
        };
        dbContext.Calendars.Add(entity);

        await SaveChangesAsync(cancellationToken);
        
        await SendToDomainEventBus(new CalendarCreatedDomainEvent
        {
            Name = command.Request.Name,
            CreatedByFamilyMemberId = familyMemberId,
            CreatedForFamilyMemberId = familyMemberId
        }, cancellationToken);
        
        return new CreateCalendarResponse
        {
            CalendarId = entity.Id
        };
    }
}

public class CreateCalendarResponse
{
    public Guid CalendarId { get; init; }
}