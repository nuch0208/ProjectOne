using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace ProjectOne.Auth
{
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
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

                var claims = new List<Claim> { new(ClaimTypes.Name, user ?? "admin") };
                if (!string.IsNullOrWhiteSpace(role)) claims.Add(new(ClaimTypes.Role, role));

                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "LocalStorageAuth")));
            }
            catch
            {
                // ตอน prerender JS ยังไม่พร้อม → ถือว่ายังไม่ login ไปก่อน
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        // เรียกหลัง login สำเร็จ
        public async Task NotifyUserAuthenticatedAsync()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        // เรียกตอน logout
        public async Task NotifyUserLoggedOutAsync()
        {
            await _localStorage.RemoveItemAsync("auth_token");
            await _localStorage.RemoveItemAsync("auth_role");
            await _localStorage.RemoveItemAsync("auth_username");

            NotifyAuthenticationStateChanged(
                Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())))
            );
        }
    }
}