using backend.Models;

namespace backend.Services.Interfaces
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) GenerateToken(User user);
    }
}
