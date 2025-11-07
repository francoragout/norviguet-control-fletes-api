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
    [Authorize]
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
        public async Task<ActionResult<List<InvoiceDto>>> GetPaymentOrdersByOrderId(int orderId)
        {
            var paymentOrders = await _context.PaymentOrders
                .Where(po => po.OrderId == orderId)
                .Include(po => po.Carrier)
                .Include(po => po.Order)
                .ToListAsync();
            var result = _mapper.Map<List<PaymentOrderDto>>(paymentOrders);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Payments")]
        public async Task<ActionResult<PaymentOrderDto>> GetPaymentOrder(int id)
        {

            var paymentOrder = await _context.PaymentOrders
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
            var order = await _context.Orders.FindAsync(dto.OrderId);
            if (order == null)
                return NotFound();

            if (order.Status == OrderStatus.Closed || order.Status == OrderStatus.Rejected)
                return Conflict(new
                {
                    code = "CANNOT_CREATE_PAYMENT_ORDER_FOR_CLOSED_OR_REJECTED_ORDER",
                    message = "No se puede crear una orden de pago para una orden cerrada o rechazada."
                });

            if (await _context.PaymentOrders.AnyAsync(po => po.PaymentOrderNumber == dto.PaymentOrderNumber))
                return Conflict(new
                {
                    code = "PAYMENT_ORDER_NUMBER_ALREADY_EXISTS",
                    message = "Payment order number already exists."
                });

            if (await _context.PaymentOrders.AnyAsync(po => po.CarrierId == dto.CarrierId && po.OrderId == dto.OrderId))
                return Conflict(new
                {
                    code = "PAYMENT_ORDER_CARRIER_ORDER_ALREADY_EXISTS",
                    message = "A payment order for this carrier and order already exists."
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
            if (await _context.PaymentOrders.AnyAsync(po => po.PaymentOrderNumber == dto.PaymentOrderNumber && po.Id != id))
                return Conflict(new
                {
                    code = "PAYMENT_ORDER_NUMBER_ALREADY_EXISTS",
                    message = "Payment order number already exists."
                });

            if (await _context.PaymentOrders.AnyAsync(po => po.CarrierId == dto.CarrierId && po.OrderId == dto.OrderId && po.Id != id))
                return Conflict(new
                {
                    code = "PAYMENT_ORDER_CARRIER_ORDER_ALREADY_EXISTS",
                    message = "A payment order for this carrier and order already exists."
                });

            var existingPaymentOrder = await _context.PaymentOrders
                .Include(po => po.Order)
                .FirstOrDefaultAsync(po => po.Id == id);
            if (existingPaymentOrder == null)
                return NotFound();

            if (existingPaymentOrder.Order.Status == OrderStatus.Closed || existingPaymentOrder.Order.Status == OrderStatus.Rejected)
            {
                return Conflict(new
                {
                    code = "CANNOT_EDIT_CLOSED_OR_REJECTED_ORDER_PAYMENT_ORDER",
                    message = "Cannot update payment order from a closed or rejected order."
                });
            }

            _mapper.Map(dto, existingPaymentOrder);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, Payments")]
        public async Task<IActionResult> DeletePaymentOrder(int id)
        {
            var paymentOrder = await _context.PaymentOrders
                .Include(po => po.Order)
                .FirstOrDefaultAsync(po => po.Id == id);
            if (paymentOrder == null)
                return NotFound();
            if (paymentOrder.Order.Status == OrderStatus.Closed)
            {
                return Conflict(new
                {
                    code = "CANNOT_DELETE_CLOSED_ORDER_PAYMENT_ORDER",
                    message = "Cannot delete payment order from a closed order."
                });
            }
            _context.PaymentOrders.Remove(paymentOrder);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("bulk")]
        [Authorize(Roles = "Admin, Payments")]
        public async Task<IActionResult> DeletePaymentOrdersBulk([FromBody] DeleteEntitiesDto dto)
        {
            var paymentOrders = await _context.PaymentOrders
                .Where(po => dto.Ids.Contains(po.Id))
                .Include(po => po.Order)
                .ToListAsync();
            if (!paymentOrders.Any())
                return NotFound();

            if (paymentOrders.Any(po => po.Order.Status == OrderStatus.Closed))
            {
                return Conflict(new
                {
                    code = "CANNOT_DELETE_PAYMENT_ORDERS_CLOSED_ORDERS",
                    message = "Cannot delete payment orders because at least one is associated with a closed order."
                });
            }

            _context.PaymentOrders.RemoveRange(paymentOrders);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
