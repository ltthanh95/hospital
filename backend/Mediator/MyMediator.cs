using backend.Mediator.Interfaces;

namespace backend.Mediator
{
    public class MyMediator : IMyMediator
    {
        private readonly IServiceProvider _serviceProvider;

        public MyMediator(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            dynamic handler = _serviceProvider.GetService(handlerType)
                ?? throw new InvalidOperationException($"No handler registered for {request.GetType().Name}");
            return await handler.HandleAsync((dynamic)request, cancellationToken);
        }
    }
}
