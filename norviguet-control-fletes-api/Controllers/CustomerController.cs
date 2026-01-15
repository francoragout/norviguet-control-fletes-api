using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.Customer;
using norviguet_control_fletes_api.Models.Entities;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CustomerController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<CustomerDto>>> GetCustomers()
        {
            var customers = await _context.Customers.ToListAsync();
            var result = _mapper.Map<List<CustomerDto>>(customers);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return NotFound();
            var result = _mapper.Map<CustomerDto>(customer);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerCreateDto dto)
        {
            var customer = _mapper.Map<Customer>(dto);
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CustomerUpdateDto dto)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return NotFound();
            _mapper.Map(dto, customer);
            await _context.SaveChangesAsync();
            var resultDto = _mapper.Map<CustomerDto>(customer);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (customer == null)
                return NotFound();
            if (customer.Orders?.Any() == true)
            {
                return Conflict(new
                {
                    code = "ASSOCIATED_RECORDS",
                    message = "Customer cannot be deleted because it has associated orders."
                });
            }
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpPost("bulk-delete")]
        public async Task<IActionResult> DeleteBulk([FromBody] List<int> ids)
        {
            if (ids == null || ids.Count == 0)
                return BadRequest("Se requiere una lista de IDs.");

            await service.DeleteBulkAsync(ids);
            return NoContent(); // 204 No Content es el estándar para borrados exitosos
        }
    }
}
