// !/windows-client/kconnect/Services/AuthService.cs
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using KConnect.Core;
using KConnect.Models;

namespace KConnect.Services;

public record RegisterRequest(string Username, string Email, string Password, string? Phone = null);
public record LoginRequest(string Identifier, string Password, string? MfaCode = null);
public record TokenResponse(string AccessToken, string RefreshToken);

public class AuthService
{
    private readonly ApiClient _api = ApiClient.Instance;
    private readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task<(bool Success, string Message)> RegisterAsync(
        string username, string email, string password, string? phone = null)
    {
        try
        {
            var res = await _api.PostAsync("/auth/register", new RegisterRequest(username, email, password, phone));
            return res.IsSuccessStatusCode
                ? (true, "Account created! Check your email to verify.")
                : (false, await ExtractError(res));
        }
        catch { return (false, "Could not connect to server"); }
    }

    public async Task<(bool Success, string Message)> LoginAsync(
        string identifier, string password, string? mfaCode = null)
    {
        try
        {
            var res = await _api.PostAsync("/auth/login", new LoginRequest(identifier, password, mfaCode));
            if (!res.IsSuccessStatusCode)
                return (false, await ExtractError(res));

            var tokens = await res.Content.ReadFromJsonAsync<TokenResponse>(_json);
            if (tokens is null) return (false, "Unexpected server response");

            AppSession.Instance.AccessToken = tokens.AccessToken;
            AppSession.Instance.RefreshToken = tokens.RefreshToken;

            var me = await _api.GetAsync<User>("/auth/me");
            if (me is null) return (false, "Could not load user profile");

            AppSession.Instance.UserId = me.Id;
            AppSession.Instance.Username = me.Username;
            AppSession.Instance.Email = me.Email;
            AppSession.Instance.MfaEnabled = me.MfaEnabled;

            return (true, "");
        }
        catch { return (false, "Could not connect to server"); }
    }

    public void Logout()
    {
        AppSession.Instance.Clear();
    }

    private async Task<string> ExtractError(HttpResponseMessage res)
    {
        try
        {
            var body = await res.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(body);
            return doc.RootElement.TryGetProperty("detail", out var d) ? d.GetString() ?? "Unknown error" : "Unknown error";
        }
        catch { return "Unknown error"; }
    }
    
    public async Task ForgotPasswordAsync(string email)
    {
        try { await _api.PostAsync("/auth/forgot-password", new { email }); }
        catch { /* swallow — always show success to avoid enumeration */ }
    }
}