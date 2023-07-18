using Common.Models;

namespace GameServer.Repositories.Entities;

public class Resource
{
    public long PlayerId { get; set; }
    public ResourceType Type { get; set; }
    public long Value { get; set; }
}