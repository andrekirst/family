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
    IReadOnlyList<string> Errors)
{
    public static LoginResult Success(string accessToken, string? refreshToken, User user) =>
        new(true, accessToken, refreshToken, user, Array.Empty<string>());

    public static LoginResult Failure(params string[] errors) =>
        new(false, null, null, null, errors);

    public static LoginResult Failure(IReadOnlyList<string> errors) =>
        new(false, null, null, null, errors);
}

public record RefreshTokenResult(
    bool IsSuccess,
    string? AccessToken,
    string? RefreshToken,
    IReadOnlyList<string> Errors)
{
    public static RefreshTokenResult Success(string accessToken, string? refreshToken) =>
        new(true, accessToken, refreshToken, Array.Empty<string>());

    public static RefreshTokenResult Failure(params string[] errors) =>
        new(false, null, null, errors);

    public static RefreshTokenResult Failure(IReadOnlyList<string> errors) =>
        new(false, null, null, errors);
}

public record LogoutResult(
    bool IsSuccess,
    IReadOnlyList<string> Errors)
{
    public static LogoutResult Success() =>
        new(true, Array.Empty<string>());

    public static LogoutResult Failure(params string[] errors) =>
        new(false, errors);

    public static LogoutResult Failure(IReadOnlyList<string> errors) =>
        new(false, errors);
}