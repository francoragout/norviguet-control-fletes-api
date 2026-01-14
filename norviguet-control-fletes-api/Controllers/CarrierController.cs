using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using norviguet_control_fletes_api.Models.DTOs.Carrier;
using norviguet_control_fletes_api.Services.Interfaces;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CarrierController(ICarrierService service) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CarrierDto>>> GetCarriers()
        {
            var carriers = await service.GetAllAsync();
            return Ok(carriers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CarrierDto>> GetCarrier(int id)
        {
            var carrier = await service.GetByIdAsync(id);
            return Ok(carrier);
        }

        [HttpPost]
        public async Task<ActionResult<CarrierDto>> CreateCarrier([FromBody] CarrierCreateDto dto)
        {
            var createdCarrier = await service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetCarrier), new { id = createdCarrier.Id }, createdCarrier);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CarrierDto>> UpdateCarrier(int id, [FromBody] CarrierUpdateDto dto)
        {
            var updatedCarrier = await service.UpdateAsync(id, dto);
            return Ok(updatedCarrier);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCarrier(int id)
        {
            await service.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("bulk-delete")]
        public async Task<IActionResult> DeleteCarriersBulk([FromBody] List<int> ids)
        {
            await service.DeleteBulkAsync(ids);
            return NoContent();
        }
    }
}
