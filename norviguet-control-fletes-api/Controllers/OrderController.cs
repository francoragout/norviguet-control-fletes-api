using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.Order;
using norviguet_control_fletes_api.Models.Notification; // Asegúrate de tener este using
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Services; // Para UserRole

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [PermissionAuthorize]
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
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto dto)
        {
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
                    Title = "Nuevo pedido creado",
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
            _mapper.Map(dto, order);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
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
