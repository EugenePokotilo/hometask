namespace Common.Models.Infrastructure;

public class GameOperationTypeAttribute : Attribute
{
    public GameOperationType OperationType { get; private set; }

    public GameOperationTypeAttribute(GameOperationType operationType)
    {
        OperationType = operationType;
    }
}