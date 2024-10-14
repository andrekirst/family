namespace Api.Domain.Core.Authentication.Google;

public class GoogleLoginRequest
{
    public string EMail { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string GoogleId { get; set; } = default!;
    public string? AccessToken { get; set; }
    public string? LastName { get; set; } = default!;
    public string? FirstName { get; set; } = default!;
}