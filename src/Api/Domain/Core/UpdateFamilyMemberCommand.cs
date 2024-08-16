using Api.Controllers.Core;
using Api.Database;
using Api.Infrastructure;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Api.Domain.Core;

public record UpdateFamilyMemberCommand(Guid Id, UpdateFamilyMemberCommandModel Model) : ICommand;

public record UpdateFamilyMemberCommandModel(string FirstName, string LastName, DateTime Birthdate);

public class UpdateFamilyMemberCommandValidator : AbstractValidator<UpdateFamilyMemberCommand>
{
    public UpdateFamilyMemberCommandValidator(
        IStringLocalizer<FamilyMemberController> stringLocalizer,
        ApplicationDbContext dbContext)
    {
        RuleFor(x => x.Id)
            .MustAsync(dbContext.FamilyMembers.Exists)
            .WithMessage(stringLocalizer["FamilyMember not found"]);

        RuleFor(command => FirstName.FromRaw(command.Model.FirstName))
            .SetValidator(new FirstNameValidator(stringLocalizer))
            .OverridePropertyName(nameof(UpdateFamilyMemberCommandModel.FirstName));

        RuleFor(command => LastName.FromRaw(command.Model.LastName))
            .SetValidator(new LastNameValidator(stringLocalizer))
            .OverridePropertyName(nameof(UpdateFamilyMemberCommandModel.LastName));

        RuleFor(command => Birthdate.FromRaw(command.Model.Birthdate))
            .SetValidator(new BirthdateValidator(stringLocalizer))
            .OverridePropertyName(nameof(UpdateFamilyMemberCommandModel.Birthdate));
    }
}

public class UpdateFamilyMemberCommandHandler(
    ApplicationDbContext dbContext,
    IUnitOfWork unitOfWork,
    IMediator mediator,
    IMapper mapper)
    : ICommandHandler<UpdateFamilyMemberCommand>
{
    public async Task Handle(UpdateFamilyMemberCommand request, CancellationToken cancellationToken)
    {
        //var familyMember = _mapper.Map<FamilyMember>(request.Model);
        //_dbContext.FamilyMembers.Add(familyMember);
        var existingFamilyMember = await dbContext.FamilyMembers.SingleAsync(f => f.Id == request.Id, cancellationToken);
        mapper.Map(request.Model, existingFamilyMember);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var domainEvent = mapper.Map<FamilyMemberUpdatedDomainEvent>(existingFamilyMember);
        await mediator.Publish(domainEvent, cancellationToken);
    }
}

public class UpdateFamilyMemberCommandMappings : Profile
{
    public UpdateFamilyMemberCommandMappings()
    {
        CreateMap<UpdateFamilyMemberCommandModel, FamilyMember>();
        CreateMap<FamilyMember, FamilyMemberUpdatedDomainEvent>();
    }
}