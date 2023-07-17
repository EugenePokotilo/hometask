using GameServer.Repositories.Entities;

namespace GameServer.Repositories;

public class InMemoryPlayerRepository: IPlayerRepository
{
    //no need to use concurrent collection as player creation is controlled
    private static List<Player> Players = new List<Player>();

    private static readonly object _lock = new object();
    public Player Get(long id)
        => Players.FirstOrDefault(p => p.Id == id);

    public Player Get(string udid)
        => Players.FirstOrDefault(p => p.UdId == udid);

    public Player GetOrCreate(string udid)
    {
        var player = Get(udid);
        if (player == null)
        {
            lock (_lock)
            {
                var newPlayerId = Players.LastOrDefault()?.Id+1 ?? 1;
                player = new Player()
                {
                    Id = newPlayerId,
                    UdId = udid
                };
                Players.Add(player);
            }
        }
        return player;
    }
}