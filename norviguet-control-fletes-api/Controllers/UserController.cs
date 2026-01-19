using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.User;
using norviguet_control_fletes_api.Models.Enums;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public UserController(ApplicationDbContext context, IMapper mapper)
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

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            // Validación: no cambiar el rol del único admin
            var isAdmin = user.Role == UserRole.Admin;
            var totalAdmins = await _context.Users.CountAsync(u => u.Role == UserRole.Admin);

            // Convierte el string a enum para la comparación
            UserRole parsedRole;
            if (!Enum.TryParse(dto.Role, true, out parsedRole))
                parsedRole = UserRole.Pending;

            if (isAdmin && totalAdmins == 1 && parsedRole != UserRole.Admin)
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            // Validación: no eliminar el único admin
            var isAdmin = user.Role == UserRole.Admin;
            var totalAdmins = await _context.Users.CountAsync(u => u.Role == UserRole.Admin);
            if (isAdmin && totalAdmins == 1)
            {
                return BadRequest(new
                {
                    code = "CANNOT_DELETE_ALL_ADMINS",
                    message = "At least one admin user must remain in the system."
                });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("bulk")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUsers([FromBody] DeleteEntitiesDto dto)
        {
            var users = await _context.Users.Where(u => dto.Ids.Contains(u.Id)).ToListAsync();
            if (users.Count == 0)
                return NotFound();

            // Validación: no eliminar todos los admins
            var adminIdsToDelete = users.Where(u => u.Role == UserRole.Admin).Select(u => u.Id).ToList();
            var totalAdmins = await _context.Users.CountAsync(u => u.Role == UserRole.Admin);
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
    }
}
