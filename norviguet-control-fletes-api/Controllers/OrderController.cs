using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Common;
using norviguet_control_fletes_api.Models.Notification;
using norviguet_control_fletes_api.Models.Order;
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
                .Include(o => o.Seller)
                .Include(o => o.Customer)
                .Include(o => o.DeliveryNotes)
                .Include(o => o.Invoices)
                .Include(o => o.PaymentOrders)
                .ToListAsync();
            var result = _mapper.Map<List<OrderDto>>(orders);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Seller)
                .Include(o => o.Customer)
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
            if (await _context.Orders.AnyAsync(o => o.OrderNumber == dto.OrderNumber))
                return Conflict(new { message = "Order number already exists." });

            var order = _mapper.Map<Order>(dto);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // Guardar la orden antes de crear notificaciones

            var usersToNotify = await _context.Users
                .Where(u => u.Role != UserRole.Pending)
                .ToListAsync();

            // Crear notificaciones de forma secuencial para evitar concurrencia en DbContext
            foreach (var user in usersToNotify)
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = user.Id,
                    Title = "Nuevo Pedido",
                    Message = $"Se ha creado un nuevo pedido (ID: {order.Id}).",
                    Link = $"/dashboard/orders/{order.Id}/update"
                });
            }

            var resultDto = _mapper.Map<OrderDto>(order);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, resultDto);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderDto dto)
        {
            if (await _context.Orders.AnyAsync(o => o.OrderNumber == dto.OrderNumber))
                return Conflict(new { message = "Order number already exists." });

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            if (await _context.Orders.AnyAsync(o => o.OrderNumber == dto.OrderNumber && o.Id != id))
                return Conflict(new { message = "Order number already exists." });

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

            _mapper.Map(dto, order);

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
        public async Task<IActionResult> DeleteOrdersBulk([FromBody] DeleteEntitiesDto dto)
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
