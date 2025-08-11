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
        public async Task<ActionResult<UserDto>> Register(RegisterDto request)
        {
            var user = await _authService.RegisterAsync(request);
            if (user is null)
                return BadRequest("User with this email already exists.");

            var userDto = _mapper.Map<UserDto>(user);
            return Ok(userDto);
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>> Login(LoginDto request)
        {
            var result = await _authService.LoginAsync(request);
            if (result is null)
                return BadRequest("Invalid username or password.");

            // Set refresh token as HTTP-only, Secure cookie
            Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = result.RefreshTokenExpiresAt
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
                Secure = true,
                SameSite = SameSiteMode.Strict,
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
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(-1) // Expire immediately
            });
            return NoContent();
        }

        [Authorize]
        [HttpGet]
        public IActionResult AuthenticatedOnlyEndpoint()
        {
            return Ok("You are authenticated!");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnlyEndpoint()
        {
            return Ok("You are and admin!");
        }

    }
}
