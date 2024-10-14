namespace Api.Domain.Core.Authentication;

public class LoginResponse
{
    public string Id { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Token { get; set; } = default!;
}