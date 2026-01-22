using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using norviguet_control_fletes_api.Models.DTOs.Carrier;
using norviguet_control_fletes_api.Models.DTOs.Common;
using norviguet_control_fletes_api.Models.DTOs.Order;
using norviguet_control_fletes_api.Services.Interfaces;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CarrierController(ICarrierService service) : ControllerBase
    {
        /// <summary>
        /// Gets a paginated list of carriers.
        /// <summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<CarrierDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<CarrierDto>>> GetAll([FromQuery] PagedRequestDto dto, CancellationToken cancellationToken)
        {
            var result = await service.GetAllAsync(dto, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        ///  Gets a carrier by ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CarrierDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CarrierDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await service.GetByIdAsync(id, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Creates a new carrier.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CarrierDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CarrierDto>> Create([FromBody] CarrierCreateDto dto, CancellationToken cancellationToken)
        {
            var result = await service.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Updates an existing carrier.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CarrierDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CarrierDto>> UpdateCarrier(int id, [FromBody] CarrierUpdateDto dto, CancellationToken cancellationToken)
        {
            var result = await service.UpdateAsync(id, dto, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Deletes a carrier by ID.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCarrier(int id, CancellationToken cancellationToken)
        {
            await service.DeleteAsync(id, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Deletes multiple carriers by their IDs.
        /// </summary>
        [HttpPost("bulk-delete")]
        public async Task<IActionResult> DeleteCarriersBulk([FromBody] List<int> ids, CancellationToken cancellationToken)
        {
            await service.DeleteBulkAsync(ids, cancellationToken);
            return NoContent();
        }
    }
}
