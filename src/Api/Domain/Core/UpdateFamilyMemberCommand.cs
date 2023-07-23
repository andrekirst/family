using Api.Controllers.Core;
using Api.Database;
using Api.Database.Core;
using Api.Infrastructure;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Api.Domain.Core;

public record UpdateFamilyMemberCommand(int Id, UpdateFamilyMemberCommandModel Model) : ICommand;

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

        RuleFor(_ => FirstName.FromRaw(_.Model.FirstName))
            .SetValidator(new FirstNameValidator(stringLocalizer))
            .OverridePropertyName(nameof(UpdateFamilyMemberCommandModel.FirstName));

        RuleFor(_ => LastName.FromRaw(_.Model.LastName))
            .SetValidator(new LastNameValidator(stringLocalizer))
            .OverridePropertyName(nameof(UpdateFamilyMemberCommandModel.LastName));

        RuleFor(_ => Birthdate.FromRaw(_.Model.Birthdate))
            .SetValidator(new BirthdateValidator(stringLocalizer))
            .OverridePropertyName(nameof(UpdateFamilyMemberCommandModel.Birthdate));
    }
}

public class UpdateFamilyMemberCommandHandler : ICommandHandler<UpdateFamilyMemberCommand>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public UpdateFamilyMemberCommandHandler(
        ApplicationDbContext dbContext,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        IMapper mapper)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _mapper = mapper;
    }

    public async Task Handle(UpdateFamilyMemberCommand request, CancellationToken cancellationToken)
    {
        //var familyMember = _mapper.Map<FamilyMember>(request.Model);
        //_dbContext.FamilyMembers.Add(familyMember);
        var existingFamilyMember = await _dbContext.FamilyMembers.SingleAsync(f => f.Id == request.Id, cancellationToken);
        _mapper.Map(request.Model, existingFamilyMember);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var domainEvent = _mapper.Map<FamilyMemberUpdatedDomainEvent>(existingFamilyMember);
        await _mediator.Publish(domainEvent, cancellationToken);
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