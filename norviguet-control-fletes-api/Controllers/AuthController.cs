using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Models.Auth;
using norviguet_control_fletes_api.Models.User;
using norviguet_control_fletes_api.Services;
using norviguet_control_fletes_api.Models.Notification;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;
        private readonly NorviguetDbContext _context;
        private readonly INotificationService _notificationService;

        public AuthController(
            IAuthService authService,
            IMapper mapper,
            NorviguetDbContext context,
            INotificationService notificationService)
        {
            _authService = authService;
            _mapper = mapper;
            _context = context;
            _notificationService = notificationService;
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

            // Notificar a los administradores
            var adminUsers = await _context.Users
                .Where(u => u.Role == UserRole.Admin)
                .ToListAsync();

            foreach (var admin in adminUsers)
            {
                var notificationDto = new CreateNotificationDto
                {
                    UserId = admin.Id,
                    Title = "Nuevo usuario registrado",
                    Message = $"El usuario {user.Name} se ha registrado y está pendiente de aprobación.",
                    Link = "/dashboard/users"
                };
                await _notificationService.CreateNotificationAsync(notificationDto);
            }

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

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            // Obtener el ID del usuario autenticado
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            // Buscar el usuario en la base de datos
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { code = "USER_NOT_FOUND", message = "Usuario no encontrado." });

            // Verificar la contraseña actual
            if (!_authService.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
                return BadRequest(new { code = "INVALID_PASSWORD", message = "La contraseña actual es incorrecta." });

            // Actualizar la contraseña
            user.PasswordHash = _authService.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { code = "PASSWORD_CHANGED", message = "Contraseña actualizada correctamente." });
        }
    }
}
