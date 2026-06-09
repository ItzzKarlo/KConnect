// !/windows-client/KConnect/Services/WebSocketService.cs
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using KConnect.Core;
using KConnect.Models;

namespace KConnect.Services;

public class WebSocketService : IDisposable
{
    private ClientWebSocket? _ws;
    private CancellationTokenSource _cts = new();
    private const string WsBase = "ws://localhost:8000/ws";

    public event Action<Message>? MessageReceived;
    public bool IsConnected => _ws?.State == WebSocketState.Open;

    public async Task ConnectAsync()
    {
        if (IsConnected) return;

        _ws = new ClientWebSocket();
        _cts = new CancellationTokenSource();

        var uri = new Uri($"{WsBase}?token={AppSession.Instance.AccessToken}");
        await _ws.ConnectAsync(uri, _cts.Token);

        _ = Task.Run(ReceiveLoopAsync);
    }

    public async Task SendMessageAsync(Guid conversationId, string content)
    {
        if (!IsConnected) return;

        var payload = JsonSerializer.Serialize(new
        {
            conversation_id = conversationId.ToString(),
            content
        });

        var bytes = Encoding.UTF8.GetBytes(payload);
        await _ws.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            true,
            _cts.Token
        );
    }

    private async Task ReceiveLoopAsync()
    {
        var buffer = new byte[4096];
        try
        {
            while (_ws?.State == WebSocketState.Open)
            {
                var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                if (result.MessageType == WebSocketMessageType.Close) break;

                var raw = Encoding.UTF8.GetString(buffer, 0, result.Count);
                try
                {
                    var opts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
                    var msg = JsonSerializer.Deserialize<Message>(raw, opts);
                    if (msg != null)
                        System.Windows.Application.Current.Dispatcher.Invoke(
                            () => MessageReceived?.Invoke(msg));
                }
                catch { /* ignore malformed frames */ }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception) { /* reconnect logic can go here later */ }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _ws?.Dispose();
    }
}