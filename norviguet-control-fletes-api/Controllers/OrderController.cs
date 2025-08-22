using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.Order;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly NorviguetDbContext _context;
        private readonly IMapper _mapper;

        public OrderController(NorviguetDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<OrderDto>>> GetOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Carrier) // Incluir Carrier en la consulta
                .ToListAsync();
            var result = _mapper.Map<List<OrderDto>>(orders);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Carrier) // Incluir Carrier en la consulta
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
            // Generar DeliveryNote antes de guardar
            // Ejemplo: puedes usar el Id del seller como seriesNumber y el Id del customer como sequentialNumber, o cualquier lógica que prefieras
            order.GenerateDeliveryNote(order.SellerId, order.CustomerId);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
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
            _context.Orders.Update(order);
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
    }
}   
