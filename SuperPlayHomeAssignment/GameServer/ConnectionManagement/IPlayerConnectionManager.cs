using System.Net.WebSockets;

namespace GameServer.ConnectionManagement;

public interface IPlayerConnectionManager
{
    Task TrySend<T>(string udid, T notification) where T : class;
    void RegisterConnection(string udid, WebSocket webSocket);
    void PurgeConnection(string udid);

    bool HasValidConnection(string udid);
}