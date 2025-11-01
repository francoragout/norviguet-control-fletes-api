using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Common;
using norviguet_control_fletes_api.Models.Invoice;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly NorviguetDbContext _context;
        private readonly IMapper _mapper;

        public InvoiceController(NorviguetDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Purchasing")]
        public async Task<ActionResult<List<InvoiceDto>>> GetInvoices()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Order)
                .ToListAsync();
            var result = _mapper.Map<List<InvoiceDto>>(invoices);
            return Ok(result);
        }

        [HttpGet("{id}")]
        //[Authorize(Roles = "Admin, Purchasing")]
        public async Task<ActionResult<InvoiceDto>> GetInvoice(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Order)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (invoice == null)
                return NotFound();
            var result = _mapper.Map<InvoiceDto>(invoice);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Purchasing")]
        public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceDto dto)
        {
            var invoice = _mapper.Map<Invoice>(dto);
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Purchasing")]
        public async Task<IActionResult> UpdateInvoice(int id, [FromBody] UpdateInvoiceDto dto)
        {
            var existingInvoice = await _context.Invoices.FindAsync(id);
            if (existingInvoice == null)
                return NotFound();
            _mapper.Map(dto, existingInvoice);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, Purchasing")]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            var existingInvoice = await _context.Invoices.FindAsync(id);
            if (existingInvoice == null)
                return NotFound();
            _context.Invoices.Remove(existingInvoice);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("bulk")]
        [Authorize(Roles = "Admin, Purchasing")]
        public async Task<IActionResult> DeleteInvoicesBulk([FromBody] DeleteEntitiesDto dto)
        {
            var invoices = await _context.Invoices.Where(i => dto.Ids.Contains(i.Id)).ToListAsync();
            if (invoices.Count == 0)
                return NotFound();
            _context.Invoices.RemoveRange(invoices);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
