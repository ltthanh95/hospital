namespace backend.Mediator.Interfaces
{
    //Created the GENERIC blueprint for Handle and receive TRequest as paramenter and output is TResponse
    public interface IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
    }
}
