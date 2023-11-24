using Api.Controllers.Core;
using Api.Database;
using Api.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Api.Domain.Core;

public record CreateFamilyMemberCommand(CreateFamilyMemberCommandModel Model) : ICommand;

public record CreateFamilyMemberCommandModel(string FirstName, string LastName, DateTime Birthdate, string? AspNetUserId);

public class CreateFamilyMemberCommandValidator : AbstractValidator<CreateFamilyMemberCommand>
{
    public CreateFamilyMemberCommandValidator(IStringLocalizer<FamilyMemberController> stringLocalizer)
    {
        RuleFor(_ => FirstName.FromRaw(_.Model.FirstName))
            .SetValidator(new FirstNameValidator(stringLocalizer))
            .OverridePropertyName(nameof(CreateFamilyMemberCommandModel.FirstName));

        RuleFor(_ => LastName.FromRaw(_.Model.LastName))
            .SetValidator(new LastNameValidator(stringLocalizer))
            .OverridePropertyName(nameof(CreateFamilyMemberCommandModel.LastName));

        RuleFor(_ => Birthdate.FromRaw(_.Model.Birthdate))
            .SetValidator(new BirthdateValidator(stringLocalizer))
            .OverridePropertyName(nameof(CreateFamilyMemberCommandModel.Birthdate));
    }
}

public class CreateFamilyMemberCommandHandler : ICommandHandler<CreateFamilyMemberCommand>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public CreateFamilyMemberCommandHandler(
        ApplicationDbContext dbContext,
        IUnitOfWork unitOfWork,
        IMediator mediator)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task Handle(CreateFamilyMemberCommand request, CancellationToken cancellationToken)
    {
        var familyMember = _mapper.Map<FamilyMember>(request.Model);
        _dbContext.FamilyMembers.Add(familyMember);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var domainEvent = _mapper.Map<FamilyMemberCreatedDomainEvent>(familyMember);
        await _mediator.Publish(domainEvent, cancellationToken);
    }
}

public class CreateFamilyMemberCommandMappings
    : IMap<FamilyMember, CreateFamilyMemberCommandModel>,
        IMap<FamilyMemberCreatedDomainEvent, FamilyMember>
{
    public static FamilyMember MapTo(CreateFamilyMemberCommandModel source)
    {
        throw new NotImplementedException();
    }

    public static FamilyMemberCreatedDomainEvent MapTo(FamilyMember source)
    {
        throw new NotImplementedException();
    }
}