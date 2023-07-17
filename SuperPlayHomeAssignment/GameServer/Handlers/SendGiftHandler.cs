using Common.Models;
using GameServer.ConnectionManagement;
using GameServer.Repositories;
using GameServer.Repositories.Entities;
using Serilog;

namespace GameServer.Handlers;

public class SendGiftHandler : IGameOperationRequestHandler<SendGiftOperationRequest>
{
    private readonly CurrentUserProvider _currentUserProvider;
    private readonly IResourceRepository _resourceRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IPlayerNotificator _playerNotificator;
    private readonly ILogger<SendGiftHandler> _logger;

    public SendGiftHandler(CurrentUserProvider currentUserProvider, IResourceRepository resourceRepository, IPlayerRepository playerRepository, IPlayerNotificator playerNotificator, ILogger<SendGiftHandler> logger)
    {
        _currentUserProvider = currentUserProvider;
        _resourceRepository = resourceRepository;
        _playerRepository = playerRepository;
        _playerNotificator = playerNotificator;
        _logger = logger;
    }
    
    public async Task<IHandlerResponse> Handle(SendGiftOperationRequest data)
    {
        var receiver = _playerRepository.Get(data.FriendPlayerId);
        if (receiver == null)
        {
            throw new InvalidOperationException("Gift receiver player does not exist.");
        }
        var currentUser = _currentUserProvider.GetUserInfo();
         
        var newUserResource = _resourceRepository.WithdrawResource(currentUser.UserId, data.ResourceType, data.Value);
        Resource newReceiverResource;
        try
        {
            newReceiverResource = _resourceRepository.DepositResource(data.FriendPlayerId, data.ResourceType, data.Value);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, $"User {currentUser.UserId} has withdrew {data.ResourceType} in amount of {data.Value}, but the error occured during deposit to {data.FriendPlayerId}");
            throw;
        }
        
        _playerNotificator.Notify(data.FriendPlayerId, new GiftEvent()
        {
            Value = data.Value,
            ResourceType = data.ResourceType,
            GiftSender = currentUser.UserId,
            NewBalanceValue = newReceiverResource.Value
        });
        
        return new HandlerResponse(new ResourceBalanceResponse()
        {
            ResourceType = data.ResourceType,
            NewBalanceValue = newUserResource.Value
        });
    }
}