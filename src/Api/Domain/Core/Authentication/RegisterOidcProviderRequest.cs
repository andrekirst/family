namespace Api.Domain.Core.Authentication;

public sealed class RegisterOidcProviderRequest
{
    public string EMail { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public DateTime Birthdate { get; set; }
    public string? Username { get; set; } = default!;
    public string ProviderName { get; set; } = default!;
    public string? AccessToken { get; set; }
    // TODO Refactor when we have more identity providers
    public string? GoogleId { get; set; }
    
    public CreateFamilyMemberCommand ToCreateFamilyMemberCommand(string? aspNetUserId) =>
        new(new CreateFamilyMemberCommandModel(FirstName, LastName, Birthdate, aspNetUserId));
}