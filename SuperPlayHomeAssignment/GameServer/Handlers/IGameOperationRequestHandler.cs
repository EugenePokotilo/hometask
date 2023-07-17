namespace GameServer.Handlers;

public interface IGameOperationRequestHandler<T> where T: class
{
    Task<IHandlerResponse> Handle(T data);
}

public interface IHandlerResponse
{
}

public class EmptyHandlerResponse : IHandlerResponse
{
}

public class HandlerResponse : IHandlerResponse
{
    public HandlerResponse(object data)
    {
        Data = data;
    }
    public object Data { get; set; }
}