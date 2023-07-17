using Common.Models;

namespace GameServer.Repositories.Entities;

public class Player
{
    public long Id { get; set; }
    public string UdId { get; set; }
}

public class Resource
{
    public long PlayerId { get; set; }
    public ResourceType Type { get; set; }
    public long Value { get; set; }
}