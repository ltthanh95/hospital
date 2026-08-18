namespace backend.Mediator.Interfaces
{
    /*
     * Create blueprint for mediator methods
     * Reduces direct dependencies between classes by forcing them to communicate exclusively through a central mediator object. Instead of multiple classes talking directly to one another—creating a complex
    */
    public interface IMyMediator
    {
        Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    }
}
