using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Carrier;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        //[AccessConfiguration("View")]
        public async Task<ActionResult<List<CarrierDto>>> GetCarriers()
        {
            var carriers = await _context.Carriers.ToListAsync();
            var result = _mapper.Map<List<CarrierDto>>(carriers);
            return Ok(result);
        }

        [HttpGet("{id}")]
        //[AccessConfiguration("View")]
        public async Task<ActionResult<CarrierDto>> GetCarrier(int id)
        {
            var carrier = await _context.Carriers.FindAsync(id);
            if (carrier == null)
                return NotFound();
            var result = _mapper.Map<CarrierDto>(carrier);
            return Ok(result);
        }

        [HttpPost]
        //[AccessConfiguration("Create")]
        public async Task<ActionResult<CarrierDto>> CreateCarrier([FromBody] CreateCarrierDto dto)
        {
            var carrier = _mapper.Map<Carrier>(dto);
            _context.Carriers.Add(carrier);
            await _context.SaveChangesAsync();
            var resultDto = _mapper.Map<CarrierDto>(carrier);
            return CreatedAtAction(nameof(GetCarrier), new { id = carrier.Id }, resultDto);
        }


        [HttpPut("{id}")]
        //[AccessConfiguration("Update")]
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
        //[AccessConfiguration("Delete")]
        public async Task<IActionResult> DeleteCarrier(int id)
        {
            var carrier = await _context.Carriers.FindAsync(id);
            if (carrier == null)
                return NotFound();
            _context.Carriers.Remove(carrier);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
