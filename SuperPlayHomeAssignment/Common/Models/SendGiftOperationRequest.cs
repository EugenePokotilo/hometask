namespace Common.Models;

public class SendGiftOperationRequest
{
    public long FriendPlayerId { get; set; }
    public ResourceType ResourceType { get; set; }
    public long Value { get; set; }
}
