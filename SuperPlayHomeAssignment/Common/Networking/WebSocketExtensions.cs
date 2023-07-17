using System.Net.WebSockets;
using System.Text;
using Common.Models;
using Common.Models.Infrastructure;
using Newtonsoft.Json;

namespace Common.Networking;

public static class WebSocketExtensions
{
    //todo: use generics and json
    public static async Task<(string Message, WebSocketReceiveResult Response)?> ReceiveMessage(this WebSocket webSocket, CancellationToken token)
    {
        var responseBuffer = new byte[1024];
        var offset = 0;
        var packet = 1024;
        var bytesReceived = new ArraySegment<byte>(responseBuffer, offset, packet);
        var response = await webSocket.ReceiveAsync(bytesReceived, token);
        
        return response == null 
            ? null 
           // : (responseBuffer.Decode(response.Count), response);
            : (Encoding.UTF8.GetString(responseBuffer, offset, response.Count), response);
    }
    
    public static async Task<(GameDto Message, WebSocketReceiveResult Response)?> ReceiveGameDto(this WebSocket webSocket, CancellationToken token)
    {
        var resultString = string.Empty;
        (string Message, WebSocketReceiveResult Response)? result;
        do
        {
            result = await webSocket.ReceiveMessage(CancellationToken.None);
            if (!result.HasValue)
            {
                return null;
            }

            if (result.Value.Response.CloseStatus.HasValue)
            {
                return (null, result.Value.Response);
            }
            
            resultString += result.Value.Message;
        } while (!result.Value.Response.EndOfMessage);

        return (JsonConvert.DeserializeObject<GameDto>(result.Value.Message), result.Value.Response);
    }

    public static async Task SendMessage(this WebSocket webSocket, GameDto gameDto, CancellationToken token)
        => SendMessage(webSocket, JsonConvert.SerializeObject(gameDto), token);
    public static async Task SendMessage(this WebSocket webSocket, string message, CancellationToken token) 
        =>  await webSocket.SendAsync(message.Encode(), WebSocketMessageType.Text, true, token);
}