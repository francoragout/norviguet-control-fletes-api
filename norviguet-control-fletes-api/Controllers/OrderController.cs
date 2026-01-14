using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.Common;
using norviguet_control_fletes_api.Models.DTOs.Order;
using norviguet_control_fletes_api.Models.Entities;
using norviguet_control_fletes_api.Services;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public OrderController(
            ApplicationDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            if (await _context.Orders.AnyAsync(o => o.OrderNumber == dto.OrderNumber))
                return Conflict(new
                {
                    code = "ORDER_NUMBER_ALREADY_EXISTS",
                    message = "Order number already exists."
                });

            var order = _mapper.Map<Order>(dto);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderDto dto)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound();

            if (await _context.Orders.AnyAsync(o => o.OrderNumber == dto.OrderNumber && o.Id != id))
                return Conflict(new
                {
                    code = "ORDER_NUMBER_ALREADY_EXISTS",
                    message = "Order number already exists."
                });

            if (order.Status == OrderStatus.Closed || order.Status == OrderStatus.Rejected)
                return BadRequest(new
                {
                    code = "CLOSED_OR_REJECTED_ORDER",
                    message = "Cannot update a closed or rejected order."
                });


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

            if (order.Status == OrderStatus.Closed)
                return BadRequest(new
                {
                    code = "CLOSED_OR_REJECTED_ORDER",
                    message = "Cannot delete a closed order."
                });

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

            if (orders.Any(o => o.Status == OrderStatus.Closed))
            {
                return BadRequest(new
                {
                    code = "CLOSED_OR_REJECTED_ORDER",
                    message = "Cannot delete orders that are closed."
                });
            }

            var ordersToDelete = orders.Where(o => o.Status == OrderStatus.Rejected || o.Status == OrderStatus.Pending).ToList();
            if (ordersToDelete.Count > 0)
            {
                _context.Orders.RemoveRange(ordersToDelete);
                await _context.SaveChangesAsync();
            }
            return NoContent();
        }
    }
}
