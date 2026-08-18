using System.Reflection;
using backend.Mediator.Interfaces;

namespace backend.Mediator
{
    /// Provides extension methods for registering the mediator and its handlers in the service collection.
    public static class MyMediatorServiceCollectionExtensions
    {
        /// Registers the MyMediator and all request handlers from the specified assemblies into the service collection.
        /// <param name="services">The service collection to add the registrations to.</param>
        /// <param name="assemblies">Assemblies to scan for request handler implementations.</param>
        /// <returns>The updated service collection.</returns>
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
