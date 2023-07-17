using Common.Models.Infrastructure;

namespace Common.Models;

[GameOperationType(GameOperationType.ResourceBalanceResponse)]
public class ResourceBalanceResponse
{
    public ResourceType ResourceType { get; set; }
    public long NewBalanceValue { get; set; }
}