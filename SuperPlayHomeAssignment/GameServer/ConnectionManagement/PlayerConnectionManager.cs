using System.Collections.Concurrent;
using System.Net.WebSockets;
using Common.Models;
using Common.Models.Infrastructure;
using Common.Networking;

namespace GameServer.ConnectionManagement;

public class PlayerConnectionManager : IPlayerConnectionManager
{
    private ConcurrentDictionary<string, WebSocket> ConnectinDictionary = new ConcurrentDictionary<string, WebSocket>();

    public void RegisterConnection(string udid, WebSocket webSocket)
    {
        ConnectinDictionary[udid] = webSocket;
    }
    
    public void PurgeConnection(string udid)
    {
        ConnectinDictionary.Remove(udid, out _);
    }

    public bool HasValidConnection(string udid)
    {
        if (!ConnectinDictionary.TryGetValue(udid, out var connection))
        {
            return false;
        }

        if (connection.State != WebSocketState.Open)
        {
            PurgeConnection(udid);
            return false;
        }

        return true;
    }

    public async Task TrySend<T>(string udid, T notification) where T : class
    {
        if (ConnectinDictionary.TryGetValue(udid, out var webSocket))
        {
            await webSocket.SendMessage(new GameDto(notification), CancellationToken.None);    
        }
    }
}