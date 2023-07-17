using Common.Models;
using GameServer.Repositories.Entities;

namespace GameServer.Repositories;

public interface IResourceRepository
{
    void UpdateResources(long playerId, ResourceType resourceType, long value);
    Resource GetResourcesFor(long playerId, ResourceType resourceType);
    IEnumerable<Resource> GetResourcesFor(long playerId);
    Resource WithdrawResource(long playerId, ResourceType resourceType, long withdrawAmount);
    Resource DepositResource(long playerId, ResourceType resourceType, long depositValue);
}