using Api.Controllers.Core;
using Api.Database;
using Api.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Api.Domain.Core;

public record CreateFamilyMemberCommand(CreateFamilyMemberCommandModel Model) : ICommand;

public record CreateFamilyMemberCommandModel(
    string FirstName,
    string LastName,
    DateTime Birthdate,
    string? AspNetUserId);

public class CreateFamilyMemberCommandValidator : AbstractValidator<CreateFamilyMemberCommand>
{
    public CreateFamilyMemberCommandValidator(IStringLocalizer<FamilyMemberController> stringLocalizer)
    {
        RuleFor(command => FirstName.FromRaw(command.Model.FirstName))
            .SetValidator(new FirstNameValidator(stringLocalizer))
            .OverridePropertyName(nameof(CreateFamilyMemberCommandModel.FirstName));

        RuleFor(command => LastName.FromRaw(command.Model.LastName))
            .SetValidator(new LastNameValidator(stringLocalizer))
            .OverridePropertyName(nameof(CreateFamilyMemberCommandModel.LastName));

        RuleFor(command => Birthdate.FromRaw(command.Model.Birthdate))
            .SetValidator(new BirthdateValidator(stringLocalizer))
            .OverridePropertyName(nameof(CreateFamilyMemberCommandModel.Birthdate));
    }
}

public class CreateFamilyMemberCommandHandler(
    ApplicationDbContext dbContext,
    IUnitOfWork unitOfWork,
    IMediator mediator)
    : ICommandHandler<CreateFamilyMemberCommand>
{
    public async Task Handle(CreateFamilyMemberCommand request, CancellationToken cancellationToken)
    {
        var familyMember = CreateFamilyMemberCommandMappings.MapFromSource(request.Model);
        dbContext.FamilyMembers.Add(familyMember);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        var domainEvent = CreateFamilyMemberCommandMappings.MapFromSource(familyMember);
        await mediator.Publish(domainEvent, cancellationToken);
    }
}

public class CreateFamilyMemberCommandMappings : IMap<FamilyMember, CreateFamilyMemberCommandModel>, IMap<FamilyMemberCreatedDomainEvent, FamilyMember>
{
    public static FamilyMember MapFromSource(CreateFamilyMemberCommandModel source) =>
        new()
        {
            Birthdate = source.Birthdate,
            FirstName = source.FirstName,
            LastName = source.LastName,
            AspNetUserId = source.AspNetUserId
        };

    public static FamilyMemberCreatedDomainEvent MapFromSource(FamilyMember source)
    {
        ArgumentNullException.ThrowIfNull(source.FirstName);
        ArgumentNullException.ThrowIfNull(source.LastName);
        ArgumentNullException.ThrowIfNull(source.Birthdate);
        
        return new FamilyMemberCreatedDomainEvent(source.Id, source.FirstName, source.LastName, source.Birthdate.Value, source.AspNetUserId);
    }
}