using GameServer.Repositories;

namespace GameServer.ConnectionManagement;

public class PlayerNotificator : IPlayerNotificator
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IPlayerConnectionManager _playerConnectionManager;

    public PlayerNotificator(IPlayerRepository playerRepository, IPlayerConnectionManager playerConnectionManager)
    {
        _playerRepository = playerRepository;
        _playerConnectionManager = playerConnectionManager;
    }
    public async Task Notify<T>(long playerId, T notification) where T : class
    {
        var playerToNotify = _playerRepository.Get(playerId);
        if (playerToNotify == null)
        {
            throw new InvalidOperationException($"Player {playerId} does not exist.");
        }

        await _playerConnectionManager.TrySend(playerToNotify.UdId, notification);
    }
}