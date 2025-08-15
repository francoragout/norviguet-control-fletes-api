using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.AccessConfiguration;

namespace norviguet_control_fletes_api.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AccessConfigurationController : ControllerBase
    {
        private readonly NorviguetDbContext _context;
        private readonly IMapper _mapper;

        public AccessConfigurationController(NorviguetDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<AccessConfigurationDto>>> GetAccessConfigurations()
        {
            var configurations = await _context.AccessConfigurations.ToListAsync();
            var result = _mapper.Map<List<AccessConfigurationDto>>(configurations);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<AccessConfigurationDto>> CreateAccessConfiguration([FromBody] CreateAccessConfigurationDto dto)
        {
            var configuration = _mapper.Map<Entities.AccessConfiguration>(dto);
            _context.AccessConfigurations.Add(configuration);
            await _context.SaveChangesAsync();
            var resultDto = _mapper.Map<AccessConfigurationDto>(configuration);
            return CreatedAtAction(nameof(GetAccessConfigurations), new { id = configuration.Id }, resultDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccessConfiguration(int id)
        {
            var configuration = await _context.AccessConfigurations.FindAsync(id);
            if (configuration == null)
                return NotFound();
            _context.AccessConfigurations.Remove(configuration);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
