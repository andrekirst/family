using Family.Api.Models;

namespace Family.Api.Services;

public interface IKeycloakService
{
    Task<LoginInitiationResult> InitiateLoginAsync();
    Task<LoginResult> CompleteLoginAsync(string authorizationCode, string state);
    Task<LoginResult> DirectLoginAsync(string email, string password);
    Task<RefreshTokenResult> RefreshTokenAsync(string refreshToken);
    Task<LogoutResult> LogoutAsync(string accessToken);
    Task<User?> SyncUserFromKeycloakAsync(string accessToken);
}

public record LoginInitiationResult(string LoginUrl, string State);

public record LoginResult(
    bool IsSuccess,
    string? AccessToken,
    string? RefreshToken,
    User? User,
    IReadOnlyList<string> Errors);

public record RefreshTokenResult(
    bool IsSuccess,
    string? AccessToken,
    string? RefreshToken,
    IReadOnlyList<string> Errors);

public record LogoutResult(
    bool IsSuccess,
    IReadOnlyList<string> Errors);