using FluentValidation;
using MediatR;

namespace WebUi.Areas.Administration.FamilyMembers.Index;

public record Get : IRequest<ViewModel>;

// public class GetValidator : AbstractValidator<Get>
// {
// }

public class GetHandler : IRequestHandler<Get, ViewModel>
{
    public Task<ViewModel> Handle(Get request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ViewModel());
    }
}
