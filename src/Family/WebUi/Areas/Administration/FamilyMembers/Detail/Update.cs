using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebUi.Data;

namespace WebUi.Areas.Administration.FamilyMembers.Detail;

public record Update(ViewModel ViewModel) : IRequest<bool>;

public class UpdateValidator : AbstractValidator<Update>
{
    public UpdateValidator()
    {
        RuleFor(_ => _.ViewModel.FirstName)
            .NotNull()
            .WithMessage("FirstName is null")
            .NotEmpty()
            .WithMessage("FirstName is empty")
            .OverridePropertyName(nameof(ViewModel.FirstName));
        
        RuleFor(_ => _.ViewModel.LastName)
            .NotNull()
            .WithMessage("LastName is null")
            .NotEmpty()
            .WithMessage("LastName is empty")
            .OverridePropertyName(nameof(ViewModel.LastName));
    }
}

public class UpdateHandler : IRequestHandler<Update, bool>
{
    private readonly ApplicationDbContext _dbContext;

    public UpdateHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<bool> Handle(Update request, CancellationToken cancellationToken)
    {
        var affectedRows = await _dbContext.FamilyMembers
            .Where(member => member.Id == request.ViewModel.Id)
            .ExecuteUpdateAsync(calls => calls
                    .SetProperty(member => member.Birthdate, request.ViewModel.Birthdate)
                    .SetProperty(member => member.FirstName, request.ViewModel.FirstName)
                    .SetProperty(member => member.LastName, request.ViewModel.LastName),
                cancellationToken);

        return affectedRows == 1;
    }
}