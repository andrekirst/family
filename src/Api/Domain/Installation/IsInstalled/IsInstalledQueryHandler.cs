using MediatR;

namespace Api.Domain.Installation.IsInstalled;

public class IsInstalledQueryHandler : IRequestHandler<IsInstalledQuery, bool>
{
    public Task<bool> Handle(IsInstalledQuery request, CancellationToken cancellationToken)
        => Task.FromResult(!File.Exists(".install"));
}