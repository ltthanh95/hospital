namespace backend.Mediator.Interfaces
{
    public interface IMyMediator
    {
        Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    }
}
