using Common.Models;
using GameServer.ConnectionManagement;
using GameServer.Repositories;
using GameServer.Repositories.Entities;

namespace GameServer.Handlers;

public class SendGiftHandler : IGameOperationRequestHandler<SendGiftOperationRequest>
{
    private readonly CurrentUserProvider _currentUserProvider;
    private readonly IResourceRepository _resourceRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IPlayerNotificator _playerNotificator;

    public SendGiftHandler(CurrentUserProvider currentUserProvider, IResourceRepository resourceRepository, IPlayerRepository playerRepository, IPlayerNotificator playerNotificator)
    {
        _currentUserProvider = currentUserProvider;
        _resourceRepository = resourceRepository;
        _playerRepository = playerRepository;
        _playerNotificator = playerNotificator;
    }
    
    public async Task<IHandlerResponse> Handle(SendGiftOperationRequest data)
    {
        //TODO: implement concurrency on repository level (replace resources) to avoid race conditioning here.
        var receiver = _playerRepository.Get(data.FriendPlayerId);
        if (receiver == null)
        {
            throw new InvalidOperationException("Gift receiver player does not exist.");
        }
        var currentUser = _currentUserProvider.GetUserInfo();

        var userResources = _resourceRepository.GetResourcesFor(currentUser.UserId, data.ResourceType);
        if (userResources.Value < data.Value)
        {
            throw new InvalidOperationException(
                $"Insufficient resource amount. User {currentUser.UserId} wants to send a gift {data.ResourceType} with value {data.Value}, but only has {userResources.Value}");
        }
        var receiverResources = _resourceRepository.GetResourcesFor(data.FriendPlayerId, data.ResourceType);
        _resourceRepository.UpdateResources(currentUser.UserId, data.ResourceType, userResources.Value - data.Value);
        var newFriendResourceBalance = receiverResources.Value + data.Value;
        _resourceRepository.UpdateResources(data.FriendPlayerId, data.ResourceType, newFriendResourceBalance);
        
        _playerNotificator.Notify(data.FriendPlayerId, new GiftEvent()
        {
            Value = data.Value,
            ResourceType = data.ResourceType,
            GiftSender = currentUser.UserId,
            NewBalanceValue = newFriendResourceBalance
        });
        
        return new HandlerResponse(new ResourceBalanceResponse()
        {
            ResourceType = data.ResourceType,
            NewBalanceValue = data.Value
        });
    }
}