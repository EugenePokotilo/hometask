using GameServer.Repositories.Entities;

namespace GameServer.Repositories;

public interface IPlayerRepository
{
    Player Get(long id);
    Player Get(string udid);
    Player GetOrCreate(string udid);

}