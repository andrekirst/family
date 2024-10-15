namespace Api.Infrastructure;

public class JwtOptions
{
    public const string OptionsName = "Jwt";
    
    public int ExpirationMinutes { get; set; } = 30;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public string Sub { get; set; } = default!;
    public string IssuerSigningKey { get; set; } = default!;
}