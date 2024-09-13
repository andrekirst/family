namespace Api.Domain.Installation.Install;

public interface IUserRegistration
{
    Task<bool> Register(RegisterOptions options, CancellationToken cancellationToken = default);
}

public class UserRegistration : IUserRegistration
{
    public Task<bool> Register(RegisterOptions options, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

public class RegisterOptions
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
}