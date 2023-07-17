using Common.Models.Infrastructure;

namespace Common.Models;


[GameOperationType(GameOperationType.UpdateResourcesRequest)]
public class UpdateResourcesOperationRequest 
{
    public ResourceType ResourceType { get; set; }
    public long Value { get; set; }
}