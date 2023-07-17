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
            resource = new Resource()
            {
                PlayerId = playerId,
                Type = resourceType,
                Value = value
            };
            Resources.Add(resource);
        }
        resource.Value = value;
    }

    public Resource GetResourcesFor(long playerId, ResourceType resourceType)
    {
        var resource = Resources.FirstOrDefault(r => r.PlayerId == playerId && r.Type == resourceType);
        if (resource == null)
        {
            resource = new Resource()
            {
                PlayerId = playerId,
                Type = resourceType,
                Value = 0
            };
            Resources.Add(resource);
        }
        return resource;
    }
        
}