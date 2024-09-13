using FluentValidation;

namespace Api.Domain.Installation.Install;

public class InstallCommandValidator : AbstractValidator<InstallCommand>
{
    public InstallCommandValidator()
    {
        RuleFor(i => i.Options)
            .NotNull();

        RuleFor(i => i.Options.Username)
            .NotNull()
            .NotEmpty();

        RuleFor(i => i.Options.Password)
            .NotNull()
            .NotEmpty();
    }
}