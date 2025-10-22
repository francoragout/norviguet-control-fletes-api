using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.Order;
using norviguet_control_fletes_api.Models.Notification;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Services;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly NorviguetDbContext _context;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public OrderController(
            NorviguetDbContext context,
            IMapper mapper,
            INotificationService notificationService)
        {
            _context = context;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<ActionResult<List<OrderDto>>> GetOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Carrier) // Incluir Carrier en la consulta
                .Include(o => o.Customer) // Incluir Customer en la consulta
                .Include(o => o.Seller) // Incluir Seller en la consulta
                .ToListAsync();
            var result = _mapper.Map<List<OrderDto>>(orders);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Carrier) // Incluir Carrier en la consulta
                .Include(o => o.Customer) // Incluir Customer en la consulta
                .Include(o => o.Seller) // Incluir Seller en la consulta
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
                return NotFound();
            var result = _mapper.Map<OrderDto>(order);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto dto)
        {
            // Validar existencia de PaymentId e InvoiceId si se especifican
            if (dto.PaymentId.HasValue)
            {
                var paymentExists = await _context.Payments.AnyAsync(p => p.Id == dto.PaymentId.Value);
                if (!paymentExists)
                {
                    return BadRequest(new
                    {
                        code = "INVALID_PAYMENT_ID",
                        message = $"The PaymentId {dto.PaymentId.Value} does not exist."
                    });
                }
            }
            if (dto.InvoiceId.HasValue)
            {
                var invoiceExists = await _context.Invoices.AnyAsync(i => i.Id == dto.InvoiceId.Value);
                if (!invoiceExists)
                {
                    return BadRequest(new
                    {
                        code = "INVALID_INVOICE_ID",
                        message = $"The InvoiceId {dto.InvoiceId.Value} does not exist."
                    });
                }
            }

            var order = _mapper.Map<Entities.Order>(dto);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Notificar a todos los usuarios cuyo rol no sea Pending
            var usersToNotify = await _context.Users
                .Where(u => u.Role != UserRole.Pending)
                .ToListAsync();

            foreach (var user in usersToNotify)
            {
                var notificationDto = new CreateNotificationDto
                {
                    UserId = user.Id,
                    Title = "Nuevo Pedido",
                    Message = $"Se ha creado un nuevo pedido (ID: {order.Id}).",
                    Link = $"/dashboard/orders/{order.Id}/update"
                };
                await _notificationService.CreateNotificationAsync(notificationDto);
            }

            var resultDto = _mapper.Map<OrderDto>(order);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, resultDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderDto dto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            // Validar existencia de PaymentId e InvoiceId si se especifican
            if (dto.PaymentId.HasValue)
            {
                var paymentExists = await _context.Payments.AnyAsync(p => p.Id == dto.PaymentId.Value);
                if (!paymentExists)
                {
                    return BadRequest(new
                    {
                        code = "INVALID_PAYMENT_ID",
                        message = $"The PaymentId {dto.PaymentId.Value} does not exist."
                    });
                }
            }
            if (dto.InvoiceId.HasValue)
            {
                var invoiceExists = await _context.Invoices.AnyAsync(i => i.Id == dto.InvoiceId.Value);
                if (!invoiceExists)
                {
                    return BadRequest(new
                    {
                        code = "INVALID_INVOICE_ID",
                        message = $"The InvoiceId {dto.InvoiceId.Value} does not exist."
                    });
                }
            }

            _mapper.Map(dto, order);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            _mapper.Map(dto, order); // Usa el mapeo que convierte string a enum

            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("bulk")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteOrdersBulk([FromBody] DeleteOrdersDto dto)
        {
            var orders = await _context.Orders
                .Where(o => dto.Ids.Contains(o.Id))
                .ToListAsync();
            if (orders.Count == 0)
                return NotFound();
            _context.Orders.RemoveRange(orders);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
