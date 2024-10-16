namespace Api.Domain.Core.Authentication;

public class LoginResponse
{
    public string? Id { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Token { get; set; }
    public string? Name { get; set; }
}