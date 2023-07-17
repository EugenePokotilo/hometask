using System.Reflection;
using GameServer.Handlers;

namespace GameServer.Configurations;

public static class HandlersConfiguration
{
    private static Dictionary<Type, Type> requestHandlerMapping = new Dictionary<Type, Type>();

    public static Type GetHandlerFor(Type modelType) => requestHandlerMapping.GetValueOrDefault(modelType); 
    
    public static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        var handlers = Assembly.GetAssembly(typeof(HandlersConfiguration)).GetTypes()
            .Where(t => t.IsClass
                        && !t.IsAbstract
                        && t.GetInterfaces().FirstOrDefault(i =>
                            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IGameOperationRequestHandler<>)) != null);

        foreach (var handler in handlers)
        {
            services.AddScoped(handler);
         
            var interfaces = handler.GetInterfaces().Where(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IGameOperationRequestHandler<>));
            foreach (var @interface in interfaces)
            {
                requestHandlerMapping.Add(@interface.GenericTypeArguments.Single(), handler);
            }
        }
            
        return services;
    }
}