using System.Net.Http.Headers;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using WebUI.Helpers;

namespace WebUI;

public class FamilyAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorageService;
    private readonly AuthenticationState _anonymousState;

    public const string AuthenticationToken = "authenticationToken";
    private const string AuthenticationType = "jwpAuthType";

    public FamilyAuthenticationStateProvider(
        HttpClient httpClient,
        ILocalStorageService localStorageService)
    {
        _httpClient = httpClient;
        _localStorageService = localStorageService;
        _anonymousState = CreateAnonymousState();
    }

    private static AuthenticationState CreateAnonymousState() =>
        new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorageService.GetItemAsStringAsync(AuthenticationToken);
        if (string.IsNullOrEmpty(token))
        {
            return _anonymousState;
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(JwtParser.ParseClaimsFromJwt(token), AuthenticationType)));
    }

    public void NotifyUserAuthentication(string login)
    {
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, login) }, AuthenticationType));
        var authenticationState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authenticationState);
    }

    public void NotifyUserLogout()
    {
        var authenticationState = Task.FromResult(_anonymousState);
        NotifyAuthenticationStateChanged(authenticationState);
    }
}