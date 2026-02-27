using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace ProjectOne.Security;

public sealed class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;

    public CustomAuthStateProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>("auth_token");
            var role  = await _localStorage.GetItemAsync<string>("auth_role");
            var user  = await _localStorage.GetItemAsync<string>("auth_username");

            if (string.IsNullOrWhiteSpace(token))
                return Anonymous();

            var claims = new List<Claim>();

            if (!string.IsNullOrWhiteSpace(user))
                claims.Add(new Claim(ClaimTypes.Name, user));

            if (!string.IsNullOrWhiteSpace(role))
                claims.Add(new Claim(ClaimTypes.Role, role));

            var identity = new ClaimsIdentity(claims, "jwt");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            // ✅ ตอน prerender JS interop ยังใช้ไม่ได้ (localStorage จะพัง) -> ให้เป็น anonymous ไปก่อน
            return Anonymous();
        }
    }

    // ✅ เรียกหลังจาก interactive พร้อมแล้ว เพื่ออ่าน localStorage แล้วแจ้ง state ใหม่
    public async Task RefreshFromStorageAsync()
    {
        var state = await GetAuthenticationStateAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(state));
    }

    public void NotifyUserAuthentication(string username, string role)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        }, "jwt");

        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
    }

    public void NotifyUserLogout()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous()));
    }

    private static AuthenticationState Anonymous()
        => new(new ClaimsPrincipal(new ClaimsIdentity()));
}