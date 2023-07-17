using System.Net;
using System.Net.WebSockets;
using Common.Models;
using Common.Models.Infrastructure;
using Common.Networking;
using GameServer.Configurations;
using GameServer.ConnectionManagement;
using GameServer.Handlers;
using Newtonsoft.Json;

namespace GameServer.Middleware;


//Single connection is used (routing done within details in the model) 
//My personal pick:
//single connection: + better overall server performance (if many users -> better utilization of each connection), mb better perf for device if too many connections per device
//single connection: - no routing itself, if many operations and objects are large -> single connection throughput issues -> game performance -> ux might be degraded.  Middleware is not reused!
//multi conn: + easy routing and generic models parsing = fast development, faster game, 
//multi conn: - more concurrency, less overall perf on the server
//Connection pool: + best as can be set up for each game(best generic solution in terms of performance 
//  (but it's better to have dedicated connection for highly loaded endpoint)
//Connection pool: - dev cost, again the highest performance is hidden under understanding of the game.
public class WebSocketConnectionMiddleware
{
    private readonly RequestDelegate _next;
    
    public WebSocketConnectionMiddleware(RequestDelegate next)
    {
        this._next = next;
    }

    // IMessageWriter is injected into InvokeAsync
    public async Task InvokeAsync(HttpContext httpContext, IServiceProvider serviceProvider, IPlayerConnectionManager playerConnectionManager, CurrentUserProvider currentUserProvider)
    {
        if (httpContext.Request.Path == "/game")
        {
            if (httpContext.User == null)
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            else if (playerConnectionManager.HasValidConnection(currentUserProvider.GetUserInfo().Udid))
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            else
            {
                using (WebSocket webSocket = await httpContext.WebSockets.AcceptWebSocketAsync())
                {
                    await Pipe(webSocket, serviceProvider,playerConnectionManager, currentUserProvider.GetUserInfo().Udid);
                }    
            }
        }
        
        await _next(httpContext);
    }
    
    private async Task Pipe(WebSocket webSocket, IServiceProvider serviceProvider, IPlayerConnectionManager playerConnectionManager, string udid)
    {
        playerConnectionManager.RegisterConnection(udid, webSocket);
        
        var result = await webSocket.ReceiveGameDto(CancellationToken.None);
        while (result is { Message: { } } && !result.Value.Response.CloseStatus.HasValue)
        {
            var gameDto = result.Value.Message;
            
            var dataType = GameDto.GetDataModelType(gameDto.OperationType);
            var dataModel = JsonConvert.DeserializeObject(gameDto.Data, dataType);
            var handlerType = HandlersConfiguration.GetHandlerFor(dataType);
            
            //handler is an entry point
            //handle each received message alike a separate request with a separate dependency scope.
            var handler = serviceProvider.CreateScope().ServiceProvider.GetRequiredService(handlerType);
            var task = (Task<IHandlerResponse>)typeof(IGameOperationRequestHandler<>)
                .MakeGenericType(dataType)
                .GetMethods()
                .Single()
                .Invoke(handler, new []{dataModel})!;
            try
            {
                var response = await task;
                if (response is HandlerResponse hs)
                {
                    await webSocket.SendMessage(new GameDto(hs.Data), CancellationToken.None);   
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
            result = await webSocket.ReceiveGameDto(CancellationToken.None);
        }
        
        await webSocket.CloseAsync(result.Value.Response.CloseStatus.Value, result.Value.Response.CloseStatusDescription, CancellationToken.None);
        playerConnectionManager.PurgeConnection(udid);
    }
}
