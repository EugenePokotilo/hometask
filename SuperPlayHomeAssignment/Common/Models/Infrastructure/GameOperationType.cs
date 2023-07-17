namespace Common.Models.Infrastructure;

public enum GameOperationType
{
    UpdateResourcesRequest=0, //I'd rather rely on events: user did smth, not just updated state. Cause the server is the source of truth
    SendGiftRequest=1,
    GiftEvent=2,
    ResourceBalanceResponse=3,
    ResourcesRequest=4,
    ResourcesBalanceResponse=5,
}