namespace GameServer.ConnectionManagement;

public interface IPlayerNotificator
{
    Task Notify<T>(long playerId, T notification) where T : class;
}