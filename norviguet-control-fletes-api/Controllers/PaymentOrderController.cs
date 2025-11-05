using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Common;
using norviguet_control_fletes_api.Models.Invoice;
using norviguet_control_fletes_api.Models.Payment;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentOrderController : ControllerBase
    {
        private readonly NorviguetDbContext _context;
        private readonly IMapper _mapper;

        public PaymentOrderController(NorviguetDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("order/{orderId}")]
        [Authorize(Roles = "Admin, Payments")]
        public async Task<ActionResult<List<InvoiceDto>>> GetPaymentOrdersByOrderId(int orderId)
        {
            var paymentOrders = await _context.PaymentOrders
                .Include(po => po.Invoice)
                .Where(po => po.OrderId == orderId)
                .ToListAsync();
            var result = _mapper.Map<List<PaymentOrderDto>>(paymentOrders);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Payments")]
        public async Task<ActionResult<PaymentOrderDto>> GetPaymentOrder(int id)
        {

            var paymentOrder = await _context.PaymentOrders
                .Include(po => po.Invoice)
                .FirstOrDefaultAsync(po => po.Id == id);
            if (paymentOrder == null)
                return NotFound();
            var result = _mapper.Map<PaymentOrderDto>(paymentOrder);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Payments")]
        public async Task<IActionResult> CreatePaymentOrder([FromBody] CreatePaymentOrderDto dto)
        {
            if (await _context.PaymentOrders.AnyAsync(po => po.PaymentOrderNumber == dto.PaymentOrderNumber))
                return Conflict(new
                {
                    code = "PATMENT_ORDER_NUMBER_ALREADY_EXISTS",
                    message = "Payment order number already exists."
                });

            var paymentOrder = _mapper.Map<PaymentOrder>(dto);
            _context.PaymentOrders.Add(paymentOrder);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Payments")]
        public async Task<IActionResult> UpdatePaymentOrder(int id, [FromBody] UpdatePaymentOrderDto dto)
        {
            if (await _context.PaymentOrders.AnyAsync(po => po.PaymentOrderNumber == dto.PaymentOrderNumber))
                return Conflict(new
                {
                    code = "PATMENT_ORDER_NUMBER_ALREADY_EXISTS",
                    message = "Payment order number already exists."
                });

            var existingPaymentOrder = await _context.PaymentOrders.FindAsync(id);
            if (existingPaymentOrder == null)
                return NotFound();
            _mapper.Map(dto, existingPaymentOrder);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePaymentOrder(int id)
        {
            var paymentOrder = await _context.PaymentOrders.FindAsync(id);
            if (paymentOrder == null)
                return NotFound();
            _context.PaymentOrders.Remove(paymentOrder);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("bulk")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePaymentOrdersBulk([FromBody] DeleteEntitiesDto dto)
        {
            var paymentOrders = await _context.Customers.Where(po => dto.Ids.Contains(po.Id)).ToListAsync();
            if (!paymentOrders.Any())
                return NotFound();
            _context.Customers.RemoveRange(paymentOrders);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
