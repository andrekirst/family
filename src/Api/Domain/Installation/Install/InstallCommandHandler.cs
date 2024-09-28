using Api.Infrastructure;

namespace Api.Domain.Installation.Install;

public class InstallCommandHandler(IUserRegistration userRegistration) : ICommandHandler<InstallCommand, bool>
{
    public async Task<bool> Handle(InstallCommand request, CancellationToken cancellationToken)
    {
        var installationOptions = request.Options;
        
        var userRegistrationSuccessfully = await userRegistration.Register(new RegisterOptions
            {
                Username = installationOptions.Username,
                Password = installationOptions.Password,
                Email = installationOptions.Email,
                FirstName = installationOptions.FirstName,
                LastName  = installationOptions.LastName
            },
            cancellationToken);

        return userRegistrationSuccessfully;
    }
}