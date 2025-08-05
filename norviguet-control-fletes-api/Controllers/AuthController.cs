using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models;
using norviguet_control_fletes_api.Services;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(RegisterDto request)
        {
            var user = await authService.RegisterAsync(request);
            if (user is null)
                return BadRequest("User with this email already exists.");

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>> Login(LoginDto request)
        {
            var result = await authService.LoginAsync(request);
            if (result is null)
                return BadRequest("Invalid username or password.");

            // Set refresh token as HTTP-only, Secure cookie
            Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            // Do not return refresh token in body
            return Ok(new TokenResponseDto { AccessToken = result.AccessToken, RefreshToken = string.Empty });
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized("No refresh token cookie found.");

            var result = await authService.RefreshTokensAsync(refreshToken);
            if (result is null || result.AccessToken is null || result.RefreshToken is null)
                return Unauthorized("Invalid refresh token.");

            Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok(new TokenResponseDto { AccessToken = result.AccessToken, RefreshToken = string.Empty });
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
