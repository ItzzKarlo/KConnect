// !/windows-client/kconnect/Core/ApiClient.cs
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace KConnect.Core;

public class ApiClient
{
    private static ApiClient? _instance;
    public static ApiClient Instance => _instance ??= new ApiClient();

    private readonly HttpClient _http;
    private const string BaseUrl = "http://localhost:8000"; // swap for prod

    private readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private ApiClient()
    {
        _http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
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
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<T>(_json);
    }

    public async Task<HttpResponseMessage> PostAsync<T>(string path, T body)
    {
        AttachAuth();
        var json = JsonSerializer.Serialize(body, _json);
        return await _http.PostAsync(path, new StringContent(json, Encoding.UTF8, "application/json"));
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string path, TRequest body)
    {
        var res = await PostAsync(path, body);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<TResponse>(_json);
    }
}