namespace backend.Services.Chat
{
    public interface IChatToolExecutor
    {
        Task<string> ExecuteAsync(int patientUserId, string toolName, string argumentsJson, CancellationToken cancellationToken);
    }
}
