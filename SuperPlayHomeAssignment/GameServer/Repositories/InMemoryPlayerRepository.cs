using GameServer.Repositories.Entities;

namespace GameServer.Repositories;

public class InMemoryPlayerRepository: IPlayerRepository
{
    //no need to use concurrent collection as player creation is controlled
    private static List<Player> Players = new List<Player>();

    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
    public Player Get(long id)
    {
        _lock.EnterReadLock();
        try
        {
            return Players.FirstOrDefault(p => p.Id == id);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public Player Get(string udid)
    {
        _lock.EnterReadLock();
        try
        {
            return Players.FirstOrDefault(p => p.UdId == udid);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public Player GetOrCreate(string udid)
    {   
        _lock.EnterUpgradeableReadLock();

        try
        {
            var player = Get(udid);
            if (player != null)
            {
                return player;
            }
            else
            {
                _lock.EnterWriteLock();
                try
                {
                    var newPlayerId = Players.LastOrDefault()?.Id+1 ?? 1;
                    player = new Player()
                    {
                        Id = newPlayerId,
                        UdId = udid
                    };
                    Players.Add(player);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
                return player;
            }
        }
        finally
        {
            _lock.ExitUpgradeableReadLock();
        }
    }
}