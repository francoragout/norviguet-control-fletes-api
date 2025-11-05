using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Carrier;
using norviguet_control_fletes_api.Models.Common;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CarrierController : ControllerBase
    {
        private readonly NorviguetDbContext _context;
        private readonly IMapper _mapper;

        public CarrierController(NorviguetDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<CarrierDto>>> GetCarriers()
        {
            var carriers = await _context.Carriers.ToListAsync();
            var result = _mapper.Map<List<CarrierDto>>(carriers);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<ActionResult<CarrierDto>> GetCarrier(int id)
        {
            var carrier = await _context.Carriers.FindAsync(id);
            if (carrier == null)
                return NotFound();
            var result = _mapper.Map<CarrierDto>(carrier);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<IActionResult> CreateCarrier([FromBody] CreateCarrierDto dto)
        {
            var carrier = _mapper.Map<Carrier>(dto);
            _context.Carriers.Add(carrier);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<IActionResult> UpdateCarrier(int id, [FromBody] UpdateCarrierDto dto)
        {
            var carrier = await _context.Carriers.FindAsync(id);
            if (carrier == null)
                return NotFound();
            _mapper.Map(dto, carrier);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<IActionResult> DeleteCarrier(int id)
        {
            var carrier = await _context.Carriers.FindAsync(id);
            if (carrier == null)
                return NotFound();
            _context.Carriers.Remove(carrier);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("bulk")]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<IActionResult> DeleteCarriersBulk([FromBody] DeleteEntitiesDto dto)
        {
            var carriers = await _context.Carriers.Where(c => dto.Ids.Contains(c.Id)).ToListAsync();
            if (carriers.Count == 0)
                return NotFound();
            _context.Carriers.RemoveRange(carriers);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
