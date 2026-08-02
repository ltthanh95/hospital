using System.Reflection;
using backend.Mediator.Interfaces;

namespace backend.Mediator
{
    public static class MyMediatorServiceCollectionExtensions
    {
        public static IServiceCollection AddMyMediator(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.AddScoped<IMyMediator, MyMediator>();

            var handlerInterfaceType = typeof(IRequestHandler<,>);

            foreach (var assembly in assemblies)
            {
                var handlerImplementations = assembly.GetTypes()
                    .Where(type => type is { IsClass: true, IsAbstract: false })
                    .SelectMany(type => type.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType)
                        .Select(i => new { Service = i, Implementation = type }));

                foreach (var handler in handlerImplementations)
                {
                    services.AddScoped(handler.Service, handler.Implementation);
                }
            }

            return services;
        }
    }
}
