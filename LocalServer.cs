using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace Pussycord;

public class LocalServer
{
    private static readonly HttpListener _listener = new();
    private const string URL = "http://127.0.0.1:3000/";

    public static async Task StartAsync(CancellationToken token)
    {
        _listener.Prefixes.Add(URL);
        _listener.Start();

        while (!token.IsCancellationRequested)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    _ = ProcessWebSocket(context);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
            catch (HttpListenerException) { break; }
            catch { }
        }
    }

    private static async Task ProcessWebSocket(HttpListenerContext context)
    {
        HttpListenerWebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
        WebSocket webSocket = wsContext.WebSocket;
        byte[] buffer = new byte[4096];

        try
        {
            // 1. Ao conectar, lemos o arquivo do disco e enviamos para o JS imediatamente.
            // Isso garante que o front-end carregue o estado real.
            var currentSettings = ConfigManager.Load(); 
            string jsonString = System.Text.Json.JsonSerializer.Serialize(currentSettings);
            byte[] initialData = Encoding.UTF8.GetBytes(jsonString);
            
            await webSocket.SendAsync(new ArraySegment<byte>(initialData), WebSocketMessageType.Text, true, CancellationToken.None);

            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
                else
                {
                    // Se o JS enviar algo depois (interação do usuário), salvamos.
                    string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    ConfigManager.UpdateRaw(json);
                }
            }
        }
        catch { }
        finally { webSocket.Dispose(); }
    }

    public static void Stop()
    {
        if (_listener.IsListening) _listener.Stop();
        _listener.Close();
    }
}