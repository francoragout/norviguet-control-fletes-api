using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Account;
using norviguet_control_fletes_api.Models.User;
using norviguet_control_fletes_api.Services;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly NorviguetDbContext _context;
        private readonly IMapper _mapper;
        private readonly IBlobStorageService _blobStorageService;

        public AccountController(IAuthService authService, NorviguetDbContext context, IMapper mapper, IBlobStorageService blobStorageService)
        {
            _authService = authService;
            _context = context;
            _mapper = mapper;
            _blobStorageService = blobStorageService;
        }

        [HttpGet]
        public async Task<ActionResult<UserDto>> GetAccount()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim == null)
                return Unauthorized();
            if (!int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            var result = _mapper.Map<UserDto>(user);
            if (!string.IsNullOrEmpty(user.ImageUrl))
            {
                var fileName = Uri.IsWellFormedUriString(user.ImageUrl, UriKind.Absolute)
                    ? Path.GetFileName(new Uri(user.ImageUrl).LocalPath)
                    : user.ImageUrl;

                result.ImageUrl = _blobStorageService.GetBlobSasUrl(fileName);
            }

            return Ok(result);
        }

        [HttpPatch("name")]
        public async Task<IActionResult> UpdateAccountName([FromBody] UpdateAccountNameDto dto)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim == null)
                return Unauthorized();
            if (!int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            user.Name = dto.Name;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangeAccountPassword([FromBody] ChangeAccountPasswordDto dto)
        {
            // Obtener el ID del usuario autenticado
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            // Buscar el usuario en la base de datos
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            // Verificar la contraseña actual
            if (!_authService.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
                return BadRequest(new { code = "INVALID_PASSWORD", message = "Current password incorrect" });

            // Actualizar la contraseña
            user.PasswordHash = _authService.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("image")]
        public async Task<IActionResult> UpdateAccountImage([FromForm] IFormFile? file)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim == null)
                return Unauthorized();
            if (!int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            if (file == null || file.Length == 0)
                return BadRequest("No file was received.");

            var fileName = $"user_{userId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            try
            {
                using var stream = file.OpenReadStream();
                var imageUrl = await _blobStorageService.UploadAsync(fileName, stream, file.ContentType);

                if (string.IsNullOrEmpty(imageUrl))
                    return StatusCode(500, "Could not upload the image to storage.");

                if (!string.IsNullOrEmpty(user.ImageUrl))
                {
                    var oldFileName = Uri.IsWellFormedUriString(user.ImageUrl, UriKind.Absolute)
                        ? Path.GetFileName(new Uri(user.ImageUrl).LocalPath)
                        : user.ImageUrl;
                    await _blobStorageService.DeleteAsync(oldFileName);
                }

                user.ImageUrl = imageUrl;
                await _context.SaveChangesAsync();
                return Ok(_mapper.Map<UserDto>(user));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading the image: {ex.Message}");
            }
        }

        [HttpDelete("image")]
        public async Task<IActionResult> DeleteAccountImage()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim == null)
                return Unauthorized();
            if (!int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            if (!string.IsNullOrEmpty(user.ImageUrl))
            {
                var fileName = Uri.IsWellFormedUriString(user.ImageUrl, UriKind.Absolute)
                    ? Path.GetFileName(new Uri(user.ImageUrl).LocalPath)
                    : user.ImageUrl;
                await _blobStorageService.DeleteAsync(fileName);
                user.ImageUrl = null;
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAccount()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim == null)
                return Unauthorized();
            if (!int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            if (user.Role == UserRole.Admin)
            {
                var adminCount = await _context.Users.CountAsync(u => u.Role == UserRole.Admin);
                if (adminCount <= 1)
                {
                    return BadRequest(new { code = "CANNOT_DELETE_ALL_ADMINS", message = "At least one admin user must remain in the system." });
                }
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
