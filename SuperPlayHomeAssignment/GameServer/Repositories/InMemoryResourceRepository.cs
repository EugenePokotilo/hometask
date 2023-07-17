using System.Collections.Concurrent;
using System.Threading.Tasks.Sources;
using Common.Models;
using GameServer.Repositories.Entities;

namespace GameServer.Repositories;

public class InMemoryResourceRepository : IResourceRepository
{
    public static readonly List<Resource> Resources = new List<Resource>();

    //todo: impl concurrency - consider granular locking mechanism
    public void UpdateResources(long playerId, ResourceType resourceType, long value)
    {
        var resource = Resources.FirstOrDefault(r => r.PlayerId == playerId && r.Type == resourceType);
        if (resource == null)
        {
            resource = Create(playerId, resourceType);  //race conditioning.  Granular locking is needed
        }
        resource.Value = value;
    }

    public Resource GetResourcesFor(long playerId, ResourceType resourceType)
    {
        var resource = Resources.FirstOrDefault(r => r.PlayerId == playerId && r.Type == resourceType);
        if (resource == null)
        {
            resource = Create(playerId, resourceType);
        }
        return resource;
    }

    public IEnumerable<Resource> GetResourcesFor(long playerId)
    {
        var resources = Resources.Where(r => r.PlayerId == playerId).ToList();
        
        if (resources.FirstOrDefault(r => r.Type == ResourceType.Coins) == null)
        {
            resources.Add(Create(playerId, ResourceType.Coins));
        }
        
        if (resources.FirstOrDefault(r => r.Type == ResourceType.Rolls) == null)
        {
            resources.Add(Create(playerId, ResourceType.Rolls));
        }
        
        return resources;
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
}