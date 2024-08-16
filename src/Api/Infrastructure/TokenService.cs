using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Api.Infrastructure;

public interface ITokenService
{
    string CreateToken(IdentityUser user, Guid familyMemberId);
}

public class TokenService(
    ISystemClock systemClock,
    IOptions<JwtOptions> jwtOptions,
    ILogger<TokenService> logger)
    : ITokenService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public string CreateToken(IdentityUser user, Guid familyMemberId)
    {
        var expiration = systemClock.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes).DateTime;

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(CreateClaims(user, familyMemberId)),
            Expires = expiration,
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.IssuerSigningKey)), SecurityAlgorithms.HmacSha256)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = tokenHandler.WriteToken(token);
        return jwtToken;
    }

    private IEnumerable<Claim> CreateClaims(IdentityUser user, Guid familyMemberId)
    {
        try
        {
            return new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, _jwtOptions.Sub),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, systemClock.UtcNow.ToString(CultureInfo.InvariantCulture)),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ApplicationClaimNames.CurrentFamilyMemberId, familyMemberId.ToString(), nameof(Guid))
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            throw;
        }
    }
}

public class JwtOptions
{
    public int ExpirationMinutes { get; set; } = 30;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public string Sub { get; set; } = default!;
    public string IssuerSigningKey { get; set; } = default!;
}