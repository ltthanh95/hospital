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
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            if (result is null)
            {
                return Conflict(ApiResponse<AuthResponse>.Fail("Username is already taken.", StatusCodes.Status409Conflict));
            }

            SetAuthCookie(result.Token, result.ExpiresAt);
            return Ok(ApiResponse<AuthResponse>.Success(result));
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            if (result is null)
            {
                return Unauthorized(ApiResponse<AuthResponse>.Fail("Invalid username or password.", StatusCodes.Status401Unauthorized));
            }

            SetAuthCookie(result.Token, result.ExpiresAt);
            return Ok(ApiResponse<AuthResponse>.Success(result));
        }

        [HttpPost("logout")]
        public ActionResult<ApiResponse> Logout()
        {
            Response.Cookies.Delete(AccessTokenCookieName, new CookieOptions
            {
                HttpOnly = true,
                Secure = !_environment.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
                Path = "/",
            });

            return Ok(ApiResponse.Success("Logged out."));
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
