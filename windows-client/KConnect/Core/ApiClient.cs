// !/windows-client/kconnect/Core/ApiClient.cs
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using KConnect.Core;

namespace KConnect.Core;

public class ApiClient
{
    private static ApiClient? _instance;
    public static ApiClient Instance => _instance ??= new ApiClient();

    private readonly HttpClient _http;
    private const string BaseUrl = "http://localhost:8000";

    private readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    private ApiClient()
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout     = TimeSpan.FromSeconds(15),
        };
    }

    private void AttachAuth()
    {
        var token = AppSession.Instance.AccessToken;
        _http.DefaultRequestHeaders.Authorization = token is null
            ? null
            : new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<T?> GetAsync<T>(string path)
    {
        AttachAuth();
        var res = await _http.GetAsync(path);

        if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            if (await TryRefreshAsync())
            {
                AttachAuth();
                res = await _http.GetAsync(path);
            }
        }

        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<T>(_json);
    }

    public async Task<HttpResponseMessage> PostAsync<T>(string path, T body)
    {
        AttachAuth();
        var json = JsonSerializer.Serialize(body, _json);
        var res  = await _http.PostAsync(path, new StringContent(json, Encoding.UTF8, "application/json"));

        if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized && path != "/auth/refresh")
        {
            if (await TryRefreshAsync())
            {
                AttachAuth();
                res = await _http.PostAsync(path, new StringContent(json, Encoding.UTF8, "application/json"));
            }
        }

        return res;
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string path, TRequest body)
    {
        var res = await PostAsync(path, body);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<TResponse>(_json);
    }

    // Attempts to refresh the access token using the stored refresh token.
    // Returns true if successful.
    private async Task<bool> TryRefreshAsync()
    {
        var refreshToken = AppSession.Instance.RefreshToken;
        if (string.IsNullOrEmpty(refreshToken)) return false;

        try
        {
            var body = JsonSerializer.Serialize(new { refresh_token = refreshToken }, _json);
            var res  = await _http.PostAsync("/auth/refresh",
                new StringContent(body, Encoding.UTF8, "application/json"));

            if (!res.IsSuccessStatusCode) return false;

            var tokens = await res.Content.ReadFromJsonAsync<TokenPair>(_json);
            if (tokens is null) return false;

            AppSession.Instance.AccessToken  = tokens.AccessToken;
            AppSession.Instance.RefreshToken = tokens.RefreshToken;
            return true;
        }
        catch { return false; }
    }

    private record TokenPair(string AccessToken, string RefreshToken);
}