using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using norviguet_control_fletes_api.Models.DTOs.Carrier;
using norviguet_control_fletes_api.Models.DTOs.Common;
using norviguet_control_fletes_api.Services;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CarrierController : ControllerBase
    {
        private readonly ICarrierService _carrierService;

        public CarrierController(ICarrierService carrierService)
        {
            _carrierService = carrierService;
        }

        [HttpGet]
        public async Task<ActionResult<List<CarrierDto>>> GetCarriers()
        {
            var result = await _carrierService.GetAllCarriersAsync();
            
            if (!result.IsSuccess)
                return StatusCode(500, new { message = result.ErrorMessage, code = result.ErrorCode });
            
            return Ok(result.Data);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<ActionResult<CarrierDto>> GetCarrier(int id)
        {
            var result = await _carrierService.GetCarrierByIdAsync(id);
            
            if (!result.IsSuccess)
            {
                if (result.ErrorCode == "NOT_FOUND")
                    return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
                
                return StatusCode(500, new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            
            return Ok(result.Data);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<IActionResult> CreateCarrier([FromBody] CreateCarrierDto dto)
        {
            var result = await _carrierService.CreateCarrierAsync(dto);
            
            if (!result.IsSuccess)
                return StatusCode(500, new { message = result.ErrorMessage, code = result.ErrorCode });
            
            return CreatedAtAction(nameof(GetCarrier), new { id = result.Data!.Id }, result.Data);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<IActionResult> UpdateCarrier(int id, [FromBody] UpdateCarrierDto dto)
        {
            var result = await _carrierService.UpdateCarrierAsync(id, dto);
            
            if (!result.IsSuccess)
            {
                if (result.ErrorCode == "NOT_FOUND")
                    return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
                
                return StatusCode(500, new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            
            return Ok(result.Data);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<IActionResult> DeleteCarrier(int id)
        {
            var result = await _carrierService.DeleteCarrierAsync(id);
            
            if (!result.IsSuccess)
            {
                if (result.ErrorCode == "NOT_FOUND")
                    return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
                
                if (result.ErrorCode == "ASSOCIATED_RECORDS")
                    return Conflict(new { message = result.ErrorMessage, code = result.ErrorCode });
                
                return StatusCode(500, new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            
            return NoContent();
        }

        [HttpDelete("bulk")]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<IActionResult> DeleteCarriersBulk([FromBody] DeleteEntitiesDto dto)
        {
            var result = await _carrierService.DeleteCarriersBulkAsync(dto.Ids);
            
            if (!result.IsSuccess)
            {
                if (result.ErrorCode == "NOT_FOUND")
                    return NotFound(new { message = result.ErrorMessage, code = result.ErrorCode });
                
                if (result.ErrorCode == "ASSOCIATED_RECORDS")
                    return Conflict(new { message = result.ErrorMessage, code = result.ErrorCode });
                
                return StatusCode(500, new { message = result.ErrorMessage, code = result.ErrorCode });
            }
            
            return NoContent();
        }

        [HttpGet("by-order/{orderId}/without-invoices")]
        [Authorize(Roles = "Admin, Purchasing")]
        public async Task<ActionResult<List<CarrierDto>>> GetCarriersWithoutInvoicesByOrderId(int orderId)
        {
            var result = await _carrierService.GetCarriersWithoutInvoicesByOrderIdAsync(orderId);
            
            if (!result.IsSuccess)
                return StatusCode(500, new { message = result.ErrorMessage, code = result.ErrorCode });
            
            return Ok(result.Data);
        }

        [HttpGet("by-order/{orderId}/without-payment-orders")]
        [Authorize(Roles = "Admin, Payments")]
        public async Task<ActionResult<List<CarrierDto>>> GetCarriersWithoutPaymentOrdersByOrderId(int orderId)
        {
            var result = await _carrierService.GetCarriersWithoutPaymentOrdersByOrderIdAsync(orderId);
            
            if (!result.IsSuccess)
                return StatusCode(500, new { message = result.ErrorMessage, code = result.ErrorCode });
            
            return Ok(result.Data);
        }
    }
}
