using backend.Mediator.Interfaces;

namespace backend.Mediator
{
    /// Implementation of the mediator pattern to decouple request handling logic from the request sender.
    /// This class acts as a central hub for processing requests and delegating them to the appropriate handlers.
    /// It uses ASP.NET Core's dependency injection to resolve the handler for a given request type.
    public class MyMediator : IMyMediator
    {
        // Injected service provider to resolve request handlers dynamically at runtime.
        private readonly IServiceProvider _serviceProvider;

        /// Initializes a new instance of the <see cref="MyMediator"/> class.
        /// <param name="serviceProvider">The service provider used to resolve request handlers.</param>
        ///
        public MyMediator(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        /// Sends a request to the appropriate handler and returns the response.
        /// <typeparam name="TResponse">The type of the response expected from the handler.</typeparam>
        /// <param name="request">The request object to be processed.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the handler's response.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no handler is registered for the request type.</exception>
        public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            // Determine the handler type for the given request and response types.
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));

            // Resolve the handler from the service provider.
            dynamic handler = _serviceProvider.GetService(handlerType) ?? throw new InvalidOperationException($"No handler registered for {request.GetType().Name}");

            // Invoke the handler's HandleAsync method to process the request.
            return await handler.HandleAsync((dynamic)request, cancellationToken);
        }
    }
}
