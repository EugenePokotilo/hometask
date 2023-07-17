using Common.Models;
using GameServer.Repositories;

namespace GameServer.Handlers;

public class ResourcesRequestHandler : IGameOperationRequestHandler<ResourceBalanceRequest>
{
    private readonly CurrentUserProvider _currentUserProvider;
    private readonly IResourceRepository _resourceRepository;

    public ResourcesRequestHandler(CurrentUserProvider currentUserProvider, IResourceRepository resourceRepository)
    {
        _currentUserProvider = currentUserProvider;
        _resourceRepository = resourceRepository;
    }
    
    public async Task<IHandlerResponse> Handle(ResourceBalanceRequest data)
    {
        var currentUser = _currentUserProvider.GetUserInfo();
        return new HandlerResponse(new ResourcesBalanceResponse()
        {
            Resources = _resourceRepository.GetResourcesFor(currentUser.UserId)
                .Select(r =>
                    new ResourceBalanceResponse()
                    {
                        ResourceType = r.Type,
                        NewBalanceValue = r.Value
                    }).ToList()
        });
    }
}