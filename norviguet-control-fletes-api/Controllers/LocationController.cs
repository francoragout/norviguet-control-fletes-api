using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.Location;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly NorviguetDbContext _context;
        private readonly IMapper _mapper;

        public LocationController(NorviguetDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<LocationDto>>> GetLocations()
        {
            var locations = await _context.Locations.ToListAsync();
            var result = _mapper.Map<List<LocationDto>>(locations);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LocationDto>> GetLocation(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location == null)
                return NotFound();
            var result = _mapper.Map<LocationDto>(location);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<LocationDto>> CreateLocation([FromBody] CreateLocationDto dto)
        {
            var location = _mapper.Map<Entities.Location>(dto);
            _context.Locations.Add(location);
            await _context.SaveChangesAsync();
            var resultDto = _mapper.Map<LocationDto>(location);
            return CreatedAtAction(nameof(GetLocation), new { id = location.Id }, resultDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLocation(int id, [FromBody] UpdateLocationDto dto)
        {
            var existingLocation = await _context.Locations.FindAsync(id);
            if (existingLocation == null)
                return NotFound();
            _mapper.Map(dto, existingLocation);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location == null)
                return NotFound();
            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
