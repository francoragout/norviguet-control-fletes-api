using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using norviguet_control_fletes_api.Models;
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

            // Recuperar el usuario para obtener el refresh token actualizado
            var user = await _authService.GetUserByEmailAsync(request.Email);
            if (user?.RefreshToken is null)
                return StatusCode(500, "No se pudo generar el refresh token.");

            // Set refresh token as HTTP-only, Secure cookie
            Response.Cookies.Append("refreshToken", user.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            // Only return access token in body
            return Ok(new TokenResponseDto { AccessToken = result.AccessToken });
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized("No refresh token cookie found.");

            var result = await _authService.RefreshTokensAsync(refreshToken);
            if (result is null || result.AccessToken is null)
                return Unauthorized("Invalid refresh token.");

            // Recuperar el usuario para obtener el refresh token actualizado
            var user = await _authService.GetUserByRefreshTokenAsync(refreshToken);
            if (user?.RefreshToken is null)
                return StatusCode(500, "No se pudo generar el refresh token.");

            Response.Cookies.Append("refreshToken", user.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            // Only return access token in body
            return Ok(new TokenResponseDto { AccessToken = result.AccessToken });
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
