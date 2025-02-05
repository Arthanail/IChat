using System.Net.WebSockets;

namespace WebApi.Services;

public class ChatService
{
    public readonly List<WebSocket> Clients = [];
    public readonly ReaderWriterLockSlim Locker = new();

    public async Task HandleWebSocketConnection(WebSocket webSocket)
    {
        Locker.EnterWriteLock();
        try
        {
            Clients.Add(webSocket);
        }
        finally
        {
            Locker.ExitWriteLock();
        }
        
        var buffer = new byte[1024 * 4];
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!receiveResult.CloseStatus.HasValue)
        {
            receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);

        Locker.EnterWriteLock();
        try
        {
            Clients.Remove(webSocket);
        }
        finally
        {
            Locker.ExitWriteLock();
        }
    }
}