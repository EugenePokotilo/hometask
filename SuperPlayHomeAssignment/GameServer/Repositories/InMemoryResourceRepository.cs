using System.Collections.Concurrent;
using System.Threading.Tasks.Sources;
using Common.Models;
using GameServer.Repositories.Entities;

namespace GameServer.Repositories;

public class InMemoryResourceRepository : IResourceRepository
{
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
    private static readonly List<Resource> Resources = new List<Resource>();

    //todo: avoid using updates from clients
    public void UpdateResources(long playerId, ResourceType resourceType, long value)
    {
        _lock.EnterWriteLock();
        try
        {
            var resource = GetOrCreate(playerId, resourceType);
            resource.Value = value;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    
    public Resource WithdrawResource(long playerId, ResourceType resourceType, long withdrawAmount)
    {
        _lock.EnterWriteLock();
        try
        {
            var resource = GetOrCreate(playerId, resourceType);
            if (resource.Value < withdrawAmount)
            {
                throw new InvalidOperationException($"Insufficient resource amount. Player {playerId} tries to withdraw {resourceType} in amount of {withdrawAmount}, but only has {resource.Value}");
            }
            resource.Value -= withdrawAmount;
            return resource; //todo: return clones
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public Resource DepositResource(long playerId, ResourceType resourceType, long depositValue)
    {
        _lock.EnterWriteLock();
        try
        {
            var resource = GetOrCreate(playerId, resourceType);
            resource.Value += depositValue;
            return resource;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public Resource GetResourcesFor(long playerId, ResourceType resourceType)
    {
        //todo: it worth creating all player entities on player create so less upgradeable locks are used 
        _lock.EnterUpgradeableReadLock();

        try
        {
            var resource = Resources.FirstOrDefault(r => r.PlayerId == playerId && r.Type == resourceType);
            if (resource != null)
            {
                return resource;
            }
            else
            {
                _lock.EnterWriteLock();
                try
                {
                    resource = Create(playerId, resourceType);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }

                return resource;
            }
        }
        finally
        {
            _lock.ExitUpgradeableReadLock();
        }
    }

    public IEnumerable<Resource> GetResourcesFor(long playerId)
    {
        _lock.EnterUpgradeableReadLock();

        try
        {
            var resources = Resources.Where(r => r.PlayerId == playerId).ToList();
            var hasCoins = resources.FirstOrDefault(r => r.Type == ResourceType.Coins) != null;
            var hasRolls = resources.FirstOrDefault(r => r.Type == ResourceType.Rolls) != null;
            if (!hasCoins || !hasRolls)
            {
                _lock.EnterWriteLock();
                try
                {
                    if (!hasCoins)
                    {
                        resources.Add(Create(playerId, ResourceType.Coins));
                    }   
                    if (!hasRolls)
                    {
                        resources.Add(Create(playerId, ResourceType.Rolls));
                    }   
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        
            return resources;
        }
        finally
        {
            _lock.ExitUpgradeableReadLock();
        }
    }

    private Resource Create(long playerId, ResourceType type)
    {
        var r = new Resource()
        {
            PlayerId = playerId,
            Type = type,
            Value = 0
        };
        Resources.Add(r);
        return r;
    }

    private Resource GetOrCreate(long playerId, ResourceType type)
    {
        var resource = Resources.FirstOrDefault(r => r.PlayerId == playerId && r.Type == type);
        if (resource == null)
        {
            resource = Create(playerId, type); 
        }

        return resource;
    }
}