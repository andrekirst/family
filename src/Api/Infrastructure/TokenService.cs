﻿using System.Globalization;
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
    string CreateToken(IdentityUser user, int familyMemberId);
}

public class TokenService : ITokenService
{
    private readonly ISystemClock _systemClock;
    private readonly ILogger<TokenService> _logger;
    private readonly JwtOptions _jwtOptions;

    public TokenService(
        ISystemClock systemClock,
        IOptions<JwtOptions> jwtOptions,
        ILogger<TokenService> logger)
    {
        _systemClock = systemClock;
        _logger = logger;
        _jwtOptions = jwtOptions.Value;
    }

    public string CreateToken(IdentityUser user, int familyMemberId)
    {
        var expiration = _systemClock.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes).DateTime;

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

    private IEnumerable<Claim> CreateClaims(IdentityUser user, int familyMemberId)
    {
        try
        {
            return new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, _jwtOptions.Sub),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, _systemClock.UtcNow.ToString(CultureInfo.InvariantCulture)),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ApplicationClaimNames.CurrentFamilyMemberId, familyMemberId.ToString(), nameof(Int32))
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
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