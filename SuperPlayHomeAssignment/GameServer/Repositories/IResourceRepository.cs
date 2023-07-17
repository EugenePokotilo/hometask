using Common.Models;
using GameServer.Repositories.Entities;

namespace GameServer.Repositories;

public interface IResourceRepository
{
    void UpdateResources(long playerId, ResourceType resourceType, long value);
    Resource GetResourcesFor(long playerId, ResourceType resourceType);
}