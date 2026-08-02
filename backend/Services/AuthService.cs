using backend.dbContext;
using backend.Models;
using backend.Models.Dtos;
using backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly ITokenService _tokenService;

        public AuthService(ApplicationDbContext db, ITokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            var usernameTaken = await _db.Users.AnyAsync(u => u.Username == request.Username);
            if (usernameTaken)
            {
                return null;
            }

            var user = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role,
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return BuildAuthResponse(user);
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Username == request.Username);
            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return null;
            }

            return BuildAuthResponse(user);
        }

        private AuthResponse BuildAuthResponse(User user)
        {
            var (token, expiresAt) = _tokenService.GenerateToken(user);
            return new AuthResponse
            {
                Token = token,
                ExpiresAt = expiresAt,
                Username = user.Username,
                Role = user.Role,
            };
        }
    }
}
