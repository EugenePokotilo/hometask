using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Reflection.Metadata;
using Common.Models;
using GameServer.Repositories;

namespace GameServer.Handlers;

public class UpdateResourceHandler : IGameOperationRequestHandler<UpdateResourcesOperationRequest>
{
    private readonly CurrentUserProvider _currentUserProvider;
    private readonly IResourceRepository _resourceRepository;

    public UpdateResourceHandler(CurrentUserProvider currentUserProvider, IResourceRepository resourceRepository)
    {
        _currentUserProvider = currentUserProvider;
        _resourceRepository = resourceRepository;
    }
    
    //todo: just updating the state received from the client is not good, it worth sending event smth happened and update state accordingly
    public async Task<IHandlerResponse> Handle(UpdateResourcesOperationRequest data)
    {
        Console.WriteLine($"received resource: {data.Value}");
        var currentUser = _currentUserProvider.GetUserInfo();
        _resourceRepository.UpdateResources(currentUser.UserId, data.ResourceType, data.Value);
        
        return new HandlerResponse(new ResourceBalanceResponse()
        {
            ResourceType = data.ResourceType,
            NewBalanceValue = data.Value
        });
    }
}