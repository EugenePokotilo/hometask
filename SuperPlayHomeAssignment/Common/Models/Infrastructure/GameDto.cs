using System.Reflection;
using Newtonsoft.Json;

namespace Common.Models.Infrastructure;

public class GameDto
{
    public GameDto()
    {
        
    }
    public GameDto(object model)
    {
        var attr = model.GetType().GetCustomAttribute<GameOperationTypeAttribute>();
        if (attr == null)
        {
            throw new InvalidOperationException("Data models without GameOperationTypeAttribute cannot be used in GameDto.");
        }

        OperationType = attr.OperationType;
        Data = JsonConvert.SerializeObject(model);
    }
    
    public GameOperationType OperationType { get; set; }
    
    public string Data { get; set; }

    public static Type GetDataModelType(GameOperationType operationType) =>
        OperationModelMapping.GetValueOrDefault(operationType);
    
    private static readonly Dictionary<GameOperationType, Type> OperationModelMapping =
        Assembly.GetAssembly(typeof(GameDto))
            .GetTypes()
            .Select(t => new
            {
                attr = t.GetCustomAttribute<GameOperationTypeAttribute>(false),
                type = t
            })
            .Where(i => i.attr != null)
            .ToDictionary(i => i.attr.OperationType, i => i.type);
}