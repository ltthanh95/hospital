using backend.Models.Dtos;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        public const string AccessTokenCookieName = "access_token";

        private readonly IAuthService _authService;
        private readonly IWebHostEnvironment _environment;

        public AuthController(IAuthService authService, IWebHostEnvironment environment)
        {
            _authService = authService;
            _environment = environment;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            if (result is null)
            {
                return Conflict(new { message = "Username is already taken." });
            }

            SetAuthCookie(result.Token, result.ExpiresAt);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            if (result is null)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            SetAuthCookie(result.Token, result.ExpiresAt);
            return Ok(result);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete(AccessTokenCookieName, new CookieOptions
            {
                HttpOnly = true,
                Secure = !_environment.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
                Path = "/",
            });

            return NoContent();
        }

        private void SetAuthCookie(string token, DateTime expiresAt)
        {
            Response.Cookies.Append(AccessTokenCookieName, token, new CookieOptions
            {
                HttpOnly = true,
                Secure = !_environment.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
                Expires = expiresAt,
                Path = "/",
            });
        }
    }
}
