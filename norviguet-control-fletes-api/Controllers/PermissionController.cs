using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Customer;
using norviguet_control_fletes_api.Models.Permission;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly NorviguetDbContext _context;
        private readonly IMapper _mapper;

        public PermissionController(NorviguetDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<string>>> GetPermissions()
        {
            var permissions = await _context.Permissions.ToListAsync();
            var result = _mapper.Map<List<PermissionDto>>(permissions);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PermissionDto>> GetPermission(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
                return NotFound();
            var result = _mapper.Map<PermissionDto>(permission);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<PermissionDto>> CreatePermission([FromBody] CreatePermissionDto dto)
        {
            var permission = _mapper.Map<Permission>(dto);
            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();
            var resultDto = _mapper.Map<PermissionDto>(permission);
            return CreatedAtAction(nameof(GetPermission), new { id = permission.Id }, resultDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PermissionDto>> UpdatePermission(int id, [FromBody] UpdatePermissionDto dto)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
                return NotFound();
            _mapper.Map(dto, permission);
            await _context.SaveChangesAsync();
            var resultDto = _mapper.Map<PermissionDto>(permission);
            return Ok(resultDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
                return NotFound();
            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("bulk")]
        public async Task<IActionResult> DeletePermissionsBulk([FromBody] DeletePermissionsDto dto)
        {
            var permissions = await _context.Permissions.Where(p => dto.Ids.Contains(p.Id)).ToListAsync();
            if (!permissions.Any())
                return NotFound();
            _context.Permissions.RemoveRange(permissions);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
