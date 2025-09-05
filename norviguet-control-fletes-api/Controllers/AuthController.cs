using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using norviguet_control_fletes_api.Models.Auth;
using norviguet_control_fletes_api.Models.User;
using norviguet_control_fletes_api.Services;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;

        public AuthController(IAuthService authService, IMapper mapper)
        {
            _authService = authService;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDto request)
        {
            var user = await _authService.RegisterAsync(request);
            if (user is null)
                return BadRequest(new {
                    code = "USER_EXISTS",
                    message = "A user with this email already exists."
                });

            return Ok(new {
                code = "USER_CREATED",
                message = "User created successfully and is pending approval."
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>> Login(LoginDto request)
        {
            var result = await _authService.LoginAsync(request);
            if (result is null)
                return Unauthorized(new
                {
                    code = "INVALID_CREDENTIALS",
                    message = "Invalid credentials."
                });

            // Verificar si el usuario tiene el rol 'Pending'
            var user = await _authService.GetUserByEmailAsync(request.Email);
            if (user != null && user.Role.ToString() == "Pending")
                return StatusCode(403, new
                {
                    code = "USER_PENDING",
                    message = "User is pending approval."
                });

            // Set refresh token as HTTP-only, Secure cookie
            var expires = request.RememberMe
                ? DateTimeOffset.UtcNow.AddDays(30)
                : DateTimeOffset.UtcNow.AddDays(1);
            Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // false en localhost, true en producción HTTPS
                SameSite = SameSiteMode.None, // obligatorio para cross-origin
                Expires = expires
            });

            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken()
        {
            var refreshTokenValue = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshTokenValue))
                return Unauthorized("No refresh token cookie found.");

            var result = await _authService.RefreshTokensAsync(refreshTokenValue);
            if (result is null || result.AccessToken is null)
                return Unauthorized("Invalid or expired refresh token.");

            Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // false en localhost, true en producción HTTPS
                SameSite = SameSiteMode.None, // obligatorio para cross-origin
                Expires = result.RefreshTokenExpiresAt
            });

            return Ok(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _authService.InvalidateRefreshTokenAsync(refreshToken);
            }
            // Remove the refreshToken cookie by setting it expired
            Response.Cookies.Append("refreshToken", string.Empty, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(-1) // Expire immediately
            });
            return NoContent();
        }
    }
}
