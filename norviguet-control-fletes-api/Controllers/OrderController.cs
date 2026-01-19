using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using norviguet_control_fletes_api.Models.DTOs.Common;
using norviguet_control_fletes_api.Models.DTOs.Order;
using norviguet_control_fletes_api.Services.Interfaces;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController(IOrderService service) : ControllerBase
    {
        /// <summary>
        /// Gets a paginated list of orders.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<OrderDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<OrderDto>>> GetAll([FromQuery] PagedRequestDto request, CancellationToken cancellationToken)
        {
            var result = await service.GetAllAsync(request, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Gets an order by ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await service.GetByIdAsync(id, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Creates a new order.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<OrderDto>> Create([FromBody] OrderCreateDto dto, CancellationToken cancellationToken)
        {
            var result = await service.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Updates an existing order.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<OrderDto>> Update(int id, [FromBody] OrderUpdateDto dto, CancellationToken cancellationToken)
        {
            var result = await service.UpdateAsync(id, dto, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Updates the status of an order.
        /// </summary>
        [HttpPatch("{id}/status")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> UpdateStatus(int id, [FromBody] OrderStatusUpdateDto dto, CancellationToken cancellationToken)
        {
            var result = await service.UpdateStatusAsync(id, dto, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Deletes an order by ID.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await service.DeleteAsync(id, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Deletes multiple orders by their IDs.
        /// </summary>
        [HttpDelete("bulk")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> DeleteBulk([FromBody] IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            await service.DeleteBulkAsync(ids, cancellationToken);
            return NoContent();
        }
    }
}
