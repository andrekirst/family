using FluentValidation;

namespace Api.Childs.Features.Child.Commands.Create;

public class CreateChildCommandValidator : AbstractValidator<CreateChildCommand>
{
    public CreateChildCommandValidator()
    {
    }
}