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
    private const int BufferSize = 65536; // 64 KB — handles long messages safely

    public event Action<Message>?       MessageReceived;
    public event Action<TypingEvent>?   TypingReceived;
    public event Action?                Reconnected;

    public bool IsConnected => _ws?.State == WebSocketState.Open;

    private readonly JsonSerializerOptions _opts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public async Task ConnectAsync()
    {
        if (IsConnected) return;

        _ws?.Dispose();
        _ws  = new ClientWebSocket();
        _cts = new CancellationTokenSource();

        var uri = new Uri($"{WsBase}?token={AppSession.Instance.AccessToken}");
        await _ws.ConnectAsync(uri, _cts.Token);

        _ = Task.Run(ReceiveLoopAsync);
    }

    public async Task SendMessageAsync(Guid conversationId, string content)
    {
        if (!IsConnected) return;
        await SendRawAsync(new { type = "message", conversation_id = conversationId.ToString(), content });
    }

    public async Task SendTypingAsync(Guid conversationId, bool isTyping)
    {
        if (!IsConnected) return;
        await SendRawAsync(new { type = "typing", conversation_id = conversationId.ToString(), is_typing = isTyping });
    }

    private async Task SendRawAsync<T>(T payload)
    {
        try
        {
            var json  = JsonSerializer.Serialize(payload, _opts);
            var bytes = Encoding.UTF8.GetBytes(json);
            await _ws!.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _cts.Token);
        }
        catch { /* swallow — reconnect loop will handle it */ }
    }

    private async Task ReceiveLoopAsync()
    {
        var buffer = new byte[BufferSize];

        while (!_cts.IsCancellationRequested)
        {
            try
            {
                if (_ws?.State != WebSocketState.Open)
                {
                    await Task.Delay(2000, _cts.Token);
                    await ReconnectAsync();
                    continue;
                }

                // Accumulate frames until EndOfMessage
                using var ms = new System.IO.MemoryStream();
                WebSocketReceiveResult result;
                do
                {
                    result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                    if (result.MessageType == WebSocketMessageType.Close) goto disconnect;
                    ms.Write(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage);

                var raw = Encoding.UTF8.GetString(ms.ToArray());
                ProcessFrame(raw);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (WebSocketException)
            {
                // Connection dropped — reconnect after short delay
                await Task.Delay(3000, _cts.Token).ConfigureAwait(false);
                await ReconnectAsync();
            }
            catch { /* ignore other transient errors */ }
        }
        return;

        disconnect:
        await Task.Delay(3000, _cts.Token).ConfigureAwait(false);
        await ReconnectAsync();
    }

    private void ProcessFrame(string raw)
    {
        try
        {
            using var doc  = JsonDocument.Parse(raw);
            var root  = doc.RootElement;
            var type  = root.GetProperty("type").GetString();

            if (type == "message")
            {
                var msg = JsonSerializer.Deserialize<Message>(raw, _opts);
                if (msg != null)
                    System.Windows.Application.Current.Dispatcher.Invoke(
                        () => MessageReceived?.Invoke(msg));
            }
            else if (type == "typing")
            {
                var ev = JsonSerializer.Deserialize<TypingEvent>(raw, _opts);
                if (ev != null)
                    System.Windows.Application.Current.Dispatcher.Invoke(
                        () => TypingReceived?.Invoke(ev));
            }
        }
        catch { /* malformed frame — ignore */ }
    }

    private async Task ReconnectAsync()
    {
        if (_cts.IsCancellationRequested) return;
        try
        {
            _ws?.Dispose();
            _ws = new ClientWebSocket();
            var uri = new Uri($"{WsBase}?token={AppSession.Instance.AccessToken}");
            await _ws.ConnectAsync(uri, _cts.Token);
            System.Windows.Application.Current.Dispatcher.Invoke(
                () => Reconnected?.Invoke());
        }
        catch { /* will retry on next loop iteration */ }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _ws?.Dispose();
    }
}

// Lightweight event object for typing indicators
public record TypingEvent(
    string ConversationId,
    string SenderId,
    string SenderUsername,
    bool IsTyping
);