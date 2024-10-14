namespace Api.Domain.Core.Authentication;

public class RegistrationRequest
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public DateTime Birthdate { get; set; }
    public string EMail { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;

    public void InvalidatePassword() => Password = string.Empty;
    
    public CreateFamilyMemberCommand ToCreateFamilyMemberCommand(string? aspNetUserId) =>
        new(new CreateFamilyMemberCommandModel(FirstName, LastName, Birthdate, aspNetUserId));
}