using Common.Models.Infrastructure;

namespace Common.Models;

[GameOperationType(GameOperationType.GiftEvent)]
public class GiftEvent
{   
    public ResourceType ResourceType { get; set; }
    public long Value { get; set; }
    public long NewBalanceValue { get; set; }
    public long GiftSender { get; set; }
}