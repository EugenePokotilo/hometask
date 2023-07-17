namespace Common.Models;

public class UpdateResourcesOperationRequest 
{
    public ResourceType ResourceType { get; set; }
    public long Value { get; set; }
}