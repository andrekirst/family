using Api.Infrastructure;

namespace Api.Domain.Installation.Install;

public record InstallCommand(InstallationOptions Options) : ICommand<bool>;