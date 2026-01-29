using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using norviguet_control_fletes_api.Models.DTOs.Carrier;
using norviguet_control_fletes_api.Models.DTOs.Common;
using norviguet_control_fletes_api.Services.Interfaces;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarrierController(ICarrierService service) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<CarrierDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<CarrierDto>>> GetAll([FromQuery] PagedRequestDto dto, CancellationToken cancellationToken)
        {
            var result = await service.GetAllAsync(dto, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CarrierDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CarrierDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await service.GetByIdAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CarrierDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CarrierDto>> Create([FromBody] CarrierCreateDto dto, CancellationToken cancellationToken)
        {
            var result = await service.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CarrierDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CarrierDto>> Update(int id, [FromBody] CarrierUpdateDto dto, CancellationToken cancellationToken)
        {
            var result = await service.UpdateAsync(id, dto, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await service.DeleteAsync([id], cancellationToken);
            return NoContent();
        }

        [HttpDelete("bulk-delete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> BulkDelete([FromBody] List<int> ids, CancellationToken cancellationToken)
        {
            await service.DeleteAsync(ids, cancellationToken);
            return NoContent();
        }
    }
}
