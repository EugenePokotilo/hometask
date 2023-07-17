using Common.Models.Infrastructure;

namespace Common.Models;

[GameOperationType(GameOperationType.ResourcesBalanceResponse)]
public class ResourcesBalanceResponse
{
    public List<ResourceBalanceResponse> Resources { get; set; }
}