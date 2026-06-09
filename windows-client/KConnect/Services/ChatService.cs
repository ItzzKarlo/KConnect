// !/windows-client/KConnect/Services/ChatService.cs
using System.Net.Http.Json;
using System.Text.Json;
using KConnect.Core;
using KConnect.Models;

namespace KConnect.Services;

public class ChatService
{
    private readonly ApiClient _api = ApiClient.Instance;
    private readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task<List<Conversation>> GetConversationsAsync()
    {
        try { return await _api.GetAsync<List<Conversation>>("/chats") ?? new(); }
        catch { return new(); }
    }

    public async Task<List<Message>> GetMessagesAsync(Guid conversationId)
    {
        try { return await _api.GetAsync<List<Message>>($"/chats/{conversationId}/messages") ?? new(); }
        catch { return new(); }
    }

    public async Task<Conversation?> StartConversationAsync(Guid userId)
    {
        try
        {
            var res = await _api.PostAsync("/chats", new { user_id = userId });
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<Conversation>(_json);
        }
        catch { return null; }
    }
}