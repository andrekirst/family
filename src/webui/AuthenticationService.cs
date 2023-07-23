using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace WebUI;

public interface IAuthenticationService
{
    Task<LoginResponseModel> Login(LoginRequestModel request, CancellationToken cancellationToken = default);
    Task Logout();
}

public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly ILocalStorageService _localStorageService;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public AuthenticationService(
        HttpClient httpClient,
        AuthenticationStateProvider authenticationStateProvider,
        ILocalStorageService localStorageService)
    {
        _httpClient = httpClient;
        _authenticationStateProvider = authenticationStateProvider;
        _localStorageService = localStorageService;
        _jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public async Task<LoginResponseModel> Login(LoginRequestModel request, CancellationToken cancellationToken = default)
    {
        var content = JsonSerializer.Serialize(request);
        var authenticationResult = await _httpClient.PostAsJsonAsync("", content, cancellationToken);
        var authenticationContent = await authenticationResult.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<LoginResponseModel>(authenticationContent, _jsonSerializerOptions);

        ArgumentNullException.ThrowIfNull(result);

        if (!authenticationResult.IsSuccessStatusCode)
        {
            return result;
        }

        await _localStorageService.SetItemAsStringAsync(FamilyAuthenticationStateProvider.AuthenticationToken, result.Token, cancellationToken);

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.Token);

        return new LoginResponseModel
        {
            IsAuthenticationSuccessful = true
        };
    }

    public async Task Logout()
    {
        await _localStorageService.RemoveItemAsync(FamilyAuthenticationStateProvider.AuthenticationToken);
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
}

public class LoginRequestModel
{
}

public class LoginResponseModel
{
    public string Token { get; set; } = default!;
    public bool IsAuthenticationSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
}