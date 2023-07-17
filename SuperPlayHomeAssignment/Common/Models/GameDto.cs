using Newtonsoft.Json;

namespace Common.Models;

public class GameDto
{
    public GameDto()
    {
        
    }
    public GameDto(object model)
    {
        DataTypeName = model.GetType().FullName; //todo: check value
        Data = JsonConvert.SerializeObject(model); //todo: use hook to serialization settings
    }
    
    public string DataTypeName { get; set; } //because the client is .net, we can use type name, but it worth replacing it with enum 
    public string Data { get; set; }

    // public enum GameOperationType
    // {
    //     UpdateResourcesRequest=0, //I'd rather rely on events: user did smth, not just updated state. Cause the server is the source of truth
    //     SendGiftRequest=1,
    //     GiftEvent=2,
    //     ResourceBalanceResponse=3
    // }
}