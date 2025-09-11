using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.User;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [PermissionAuthorize]
    public class UserController : ControllerBase
    {
        private readonly NorviguetDbContext _context;
        private readonly IMapper _mapper;
        public UserController(NorviguetDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<UserDto>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            var result = _mapper.Map<List<UserDto>>(users);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            // Validación: no cambiar el rol del único admin
            var isAdmin = user.Role == Entities.UserRole.Admin;
            var totalAdmins = await _context.Users.CountAsync(u => u.Role == Entities.UserRole.Admin);

            // Convierte el string a enum para la comparación
            Entities.UserRole parsedRole;
            if (!Enum.TryParse(dto.Role, true, out parsedRole))
                parsedRole = Entities.UserRole.Pending;

            if (isAdmin && totalAdmins == 1 && parsedRole != Entities.UserRole.Admin)
            {
                return BadRequest(new
                {
                    code = "CANNOT_EDIT_LAST_ADMIN_ROLE",
                    message = "Cannot change the role of the only admin user."
                });
            }

            // Aplica los cambios (el mapeo también usará la conversión robusta definida en tu UserProfile)
            _mapper.Map(dto, user);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("bulk")]
        public async Task<IActionResult> DeleteUsers([FromBody] DeleteUsersDto dto)
        {
            var users = await _context.Users.Where(u => dto.Ids.Contains(u.Id)).ToListAsync();
            if (users.Count == 0)
                return NotFound();

            // Validación: no eliminar todos los admins
            var adminIdsToDelete = users.Where(u => u.Role == Entities.UserRole.Admin).Select(u => u.Id).ToList();
            var totalAdmins = await _context.Users.CountAsync(u => u.Role == Entities.UserRole.Admin);
            if (adminIdsToDelete.Count > 0 && totalAdmins - adminIdsToDelete.Count < 1)
            {
                return BadRequest(new
                {
                    code = "CANNOT_DELETE_ALL_ADMINS",
                    message = "At least one admin user must remain in the system."
                });
            }

            _context.Users.RemoveRange(users);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetMe()
        {
            // Obtener el id del usuario autenticado desde los claims
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim == null)
                return Unauthorized();
            if (!int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            var result = _mapper.Map<UserDto>(user);
            return Ok(result);
        }
    }
}
