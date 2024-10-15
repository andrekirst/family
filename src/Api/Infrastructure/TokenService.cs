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
    IOptions<JwtOptions> jwtOptions)
    : ITokenService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public string CreateToken(IdentityUser user, Guid familyMemberId)
    {
        ArgumentNullException.ThrowIfNull(user.UserName);
        var expiration = systemClock.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes).DateTime;

        var claims = new Dictionary<string, object>
        {
            { JwtRegisteredClaimNames.Sub, user.UserName },
            { JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString() }
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.IssuerSigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            Claims = claims,
            Expires = expiration,
            Subject = new ClaimsIdentity(CreateClaims(user, familyMemberId)),
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private IEnumerable<Claim> CreateClaims(IdentityUser user, Guid familyMemberId)
    {
        ArgumentException.ThrowIfNullOrEmpty(user.UserName);
        ArgumentException.ThrowIfNullOrEmpty(user.Email);
        
        return
        [
            new Claim(JwtRegisteredClaimNames.Sub, _jwtOptions.Sub),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, systemClock.UtcNow.ToString(CultureInfo.InvariantCulture)),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ApplicationClaimNames.CurrentFamilyMemberId, familyMemberId.ToString(), nameof(Guid))
        ];
    }
}