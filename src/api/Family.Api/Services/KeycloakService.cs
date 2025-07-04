using Family.Api.Authorization;
using Family.Api.Data;
using Family.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Family.Api.Services;

public class KeycloakService : IKeycloakService
{
    private const int StateLength = 32;
    
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly FamilyDbContext _context;
    private readonly ILogger<KeycloakService> _logger;

    public KeycloakService(
        HttpClient httpClient,
        IConfiguration configuration,
        FamilyDbContext context,
        ILogger<KeycloakService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _context = context;
        _logger = logger;
    }

    public Task<LoginInitiationResult> InitiateLoginAsync()
    {
        var state = GenerateRandomString(StateLength);
        var authority = _configuration["Keycloak:Authority"];
        var clientId = _configuration["Keycloak:ClientId"];
        var redirectUri = _configuration["Keycloak:RedirectUri"] ?? "http://localhost:8081/auth/callback";
        
        var loginUrl = $"{authority}/protocol/openid-connect/auth" +
                      $"?client_id={clientId}" +
                      $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                      $"&response_type=code" +
                      $"&scope=openid profile email" +
                      $"&state={state}";

        return Task.FromResult(new LoginInitiationResult(loginUrl, state));
    }

    public async Task<LoginResult> CompleteLoginAsync(string authorizationCode, string state)
    {
        try
        {
            var tokenEndpoint = _configuration["Keycloak:TokenEndpoint"];
            var clientId = _configuration["Keycloak:ClientId"];
            var clientSecret = _configuration["Keycloak:ClientSecret"];
            var redirectUri = _configuration["Keycloak:RedirectUri"] ?? "http://localhost:8081/auth/callback";

            var tokenRequest = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("client_id", clientId!),
                new KeyValuePair<string, string>("client_secret", clientSecret!),
                new KeyValuePair<string, string>("code", authorizationCode),
                new KeyValuePair<string, string>("redirect_uri", redirectUri)
            });

            var response = await _httpClient.PostAsync(tokenEndpoint, tokenRequest);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Token exchange failed: {StatusCode} - {Content}", response.StatusCode, content);
                return LoginResult.Failure("Authentication failed");
            }

            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(content);
            var accessToken = tokenResponse.GetProperty("access_token").GetString();
            var refreshToken = tokenResponse.TryGetProperty("refresh_token", out var refreshProp) 
                ? refreshProp.GetString() 
                : null;

            if (string.IsNullOrEmpty(accessToken))
            {
                return LoginResult.Failure("Invalid token response");
            }

            var user = await SyncUserFromKeycloakAsync(accessToken);
            if (user == null)
            {
                return LoginResult.Failure("Failed to sync user data");
            }

            return LoginResult.Success(accessToken, refreshToken, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing login");
            return LoginResult.Failure("Login completion failed");
        }
    }

    public async Task<LoginResult> DirectLoginAsync(string email, string password)
    {
        try
        {
            var tokenEndpoint = _configuration["Keycloak:TokenEndpoint"];
            var clientId = _configuration["Keycloak:ClientId"];
            var clientSecret = _configuration["Keycloak:ClientSecret"];

            var tokenRequest = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("client_id", clientId!),
                new KeyValuePair<string, string>("client_secret", clientSecret!),
                new KeyValuePair<string, string>("username", email),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("scope", "openid profile email")
            });

            var response = await _httpClient.PostAsync(tokenEndpoint, tokenRequest);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Direct login failed: {StatusCode} - {Content}", response.StatusCode, content);
                return LoginResult.Failure("Invalid credentials");
            }

            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(content);
            var accessToken = tokenResponse.GetProperty("access_token").GetString();
            var refreshToken = tokenResponse.TryGetProperty("refresh_token", out var refreshProp) 
                ? refreshProp.GetString() 
                : null;

            if (string.IsNullOrEmpty(accessToken))
            {
                return LoginResult.Failure("Invalid token response");
            }

            var user = await SyncUserFromKeycloakAsync(accessToken);
            if (user == null)
            {
                return LoginResult.Failure("Failed to sync user data");
            }

            // Update last login time
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return LoginResult.Success(accessToken, refreshToken, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during direct login");
            return LoginResult.Failure("Login failed");
        }
    }

    public async Task<RefreshTokenResult> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var tokenEndpoint = _configuration["Keycloak:TokenEndpoint"];
            var clientId = _configuration["Keycloak:ClientId"];
            var clientSecret = _configuration["Keycloak:ClientSecret"];

            var tokenRequest = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_id", clientId!),
                new KeyValuePair<string, string>("client_secret", clientSecret!),
                new KeyValuePair<string, string>("refresh_token", refreshToken)
            });

            var response = await _httpClient.PostAsync(tokenEndpoint, tokenRequest);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Token refresh failed: {StatusCode} - {Content}", response.StatusCode, content);
                return RefreshTokenResult.Failure("Token refresh failed");
            }

            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(content);
            var accessToken = tokenResponse.GetProperty("access_token").GetString();
            var newRefreshToken = tokenResponse.TryGetProperty("refresh_token", out var refreshProp) 
                ? refreshProp.GetString() 
                : refreshToken; // Some providers don't return new refresh token

            if (string.IsNullOrEmpty(accessToken))
            {
                return RefreshTokenResult.Failure("Invalid token response");
            }

            return RefreshTokenResult.Success(accessToken, newRefreshToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return RefreshTokenResult.Failure("Token refresh failed");
        }
    }

    public async Task<LogoutResult> LogoutAsync(string accessToken)
    {
        try
        {
            var authority = _configuration["Keycloak:Authority"];
            var clientId = _configuration["Keycloak:ClientId"];
            var clientSecret = _configuration["Keycloak:ClientSecret"];

            var logoutEndpoint = $"{authority}/protocol/openid-connect/logout";

            var logoutRequest = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", clientId!),
                new KeyValuePair<string, string>("client_secret", clientSecret!),
                new KeyValuePair<string, string>("refresh_token", accessToken) // Note: Should be refresh token in production
            });

            var response = await _httpClient.PostAsync(logoutEndpoint, logoutRequest);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Logout failed: {StatusCode}", response.StatusCode);
                return LogoutResult.Failure("Logout failed");
            }

            return LogoutResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return LogoutResult.Failure("Logout failed");
        }
    }

    public async Task<User?> SyncUserFromKeycloakAsync(string accessToken)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(accessToken);

            var subjectId = jsonToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var email = jsonToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var firstName = jsonToken.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value;
            var lastName = jsonToken.Claims.FirstOrDefault(c => c.Type == "family_name")?.Value;
            var preferredLanguage = jsonToken.Claims.FirstOrDefault(c => c.Type == "locale")?.Value ?? "de";

            if (string.IsNullOrEmpty(subjectId) || string.IsNullOrEmpty(email))
            {
                _logger.LogError("Missing required claims in JWT token");
                return null;
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.KeycloakSubjectId == subjectId);

            if (existingUser != null)
            {
                // Update existing user
                existingUser.Email = email;
                existingUser.FirstName = firstName;
                existingUser.LastName = lastName;
                existingUser.PreferredLanguage = preferredLanguage;
                existingUser.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new user
                existingUser = new User
                {
                    Id = Guid.NewGuid(),
                    KeycloakSubjectId = subjectId,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    PreferredLanguage = preferredLanguage,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(existingUser);
            }

            // Sync roles
            await SyncUserRolesAsync(existingUser, jsonToken);

            await _context.SaveChangesAsync();
            return existingUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing user from Keycloak");
            return null;
        }
    }

    private async Task SyncUserRolesAsync(User user, JwtSecurityToken jsonToken)
    {
        var familyRoles = jsonToken.Claims
            .Where(c => c.Type == Claims.FamilyRoles)
            .Select(c => c.Value)
            .ToList();

        // Remove old roles
        var existingRoles = await _context.UserRoles
            .Where(ur => ur.UserId == user.Id && ur.Source == "keycloak")
            .ToListAsync();

        _context.UserRoles.RemoveRange(existingRoles);

        // Add new roles
        foreach (var role in familyRoles)
        {
            _context.UserRoles.Add(new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RoleName = role,
                Source = "keycloak",
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    private static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}