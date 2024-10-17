namespace Api.Features.Authentication;

public class LoginOidcProviderRequest
{
    public string EMail { get; set; } = default!;
    public string AccessToken { get; set; } = default!;

    // TODO Refactor when using more identity providers
    public string GoogleId { get; set; } = default!;
}