using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using norviguet_control_fletes_api.Models.DTOs.DeliveryNote;
using norviguet_control_fletes_api.Services.Interfaces;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryNoteController(IDeliveryNoteService service) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<DeliveryNoteDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<DeliveryNoteDto>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await service.GetAllAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DeliveryNoteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DeliveryNoteDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await service.GetByIdAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(DeliveryNoteDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DeliveryNoteDto>> Create([FromBody] DeliveryNoteCreateDto dto, CancellationToken cancellationToken)
        {
            var result = await service.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(DeliveryNoteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DeliveryNoteDto>> Update(int id, [FromBody] DeliveryNoteUpdateDto dto, CancellationToken cancellationToken)
        {
            var result = await service.UpdateAsync(id, dto, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await service.DeleteAsync([id], cancellationToken);
            return NoContent();
        }

        [HttpPost("bulk-delete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BulkDelete([FromBody] IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            await service.DeleteAsync(ids, cancellationToken);
            return NoContent();
        }
    }
}
