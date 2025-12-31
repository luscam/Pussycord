using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Pussycord;

public class CdpHelper : IDisposable
{
    private readonly ClientWebSocket _ws = new();
    private int _idCounter = 1;
    private bool _isConnected = false;

    public async Task<bool> ConnectAsync(int port)
    {
        try
        {
            using HttpClient http = new();
            string json = await http.GetStringAsync($"http://127.0.0.1:{port}/json");
            var targets = JsonSerializer.Deserialize<JsonArray>(json);
            
            if (targets == null || targets.Count == 0) return false;

            string wsUrl = targets[0]?["webSocketDebuggerUrl"]?.ToString();
            if (string.IsNullOrEmpty(wsUrl)) return false;

            await _ws.ConnectAsync(new Uri(wsUrl), CancellationToken.None);
            _isConnected = true;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task SendCommandAsync(string method, object parameters = null)
    {
        if (!_isConnected || _ws.State != WebSocketState.Open) return;

        var cmd = new
        {
            id = _idCounter++,
            method = method,
            @params = parameters ?? new object()
        };

        string json = JsonSerializer.Serialize(cmd);
        byte[] buffer = Encoding.UTF8.GetBytes(json);
        
        try 
        {
            await _ws.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch { }
    }

    public void Dispose()
    {
        _ws.Dispose();
    }
}