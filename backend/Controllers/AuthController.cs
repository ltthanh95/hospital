using System.Security.Claims;
using backend.Models;
using backend.Models.Dtos;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        //Hard code for AccessToken name => will changed it into json appsettings file for better security
        public const string AccessTokenCookieName = "access_token";

        //injected AuthService and IWebHost (THat would be determine the environment - Dev or Prod)
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
            //Check any error in this case check username is taken or not
            if (result is null)
            {
                return Conflict(ApiResponse<AuthResponse>.Fail("Username is already taken.", StatusCodes.Status409Conflict));
            }
            // set Token to cookie after register successfully
            SetAuthCookie(result.Token, result.ExpiresAt);
            return Ok(ApiResponse<AuthResponse>.Success(result));
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            //Check username and password
            if (result is null)
            {
                return Unauthorized(ApiResponse<AuthResponse>.Fail("Invalid username or password.", StatusCodes.Status401Unauthorized));
            }
            // set Token to cookie after login successfully
            SetAuthCookie(result.Token, result.ExpiresAt);
            return Ok(ApiResponse<AuthResponse>.Success(result));
        }

        //This method check current user login => purposes: get appointments, etc prevent conflict to others
        [HttpGet("me")]
        [Authorize]
        public ActionResult<ApiResponse<MeResponse>> Me()
        {
            var username = User.FindFirstValue(ClaimTypes.Name)!;
            var role = User.FindFirstValue(ClaimTypes.Role)!;

            return Ok(ApiResponse<MeResponse>.Success(new MeResponse
            {
                Username = username,
                Role = Enum.Parse<Role>(role),
            }));
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
            //Method set cookies for authentication
            //Pros and cons setting cookies instead of Bearer HTTP:
            //Using cookies for authentication instead of bearer tokens in HTTP headers offers automatic browser management and better defense against XSS when using HttpOnly flags
            //Cons: Cookies are tied to specific domains and subdomains. They do not fit well when separate mobile apps, third-party clients, or multi-service APIs need access.
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
