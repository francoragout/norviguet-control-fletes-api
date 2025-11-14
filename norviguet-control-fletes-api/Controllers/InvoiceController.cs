using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Common;
using norviguet_control_fletes_api.Models.Invoice;
using norviguet_control_fletes_api.Services;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InvoiceController : ControllerBase
    {
        private readonly NorviguetDbContext _context;
        private readonly IMapper _mapper;

        public InvoiceController(
            NorviguetDbContext context, 
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<List<InvoiceDto>>> GetInvoicesByOrderId(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
                return NotFound();

            var invoices = await _context.Invoices
                .Where(i => i.OrderId == orderId)
                .Include(i => i.Carrier)
                .Include(i => i.Order)
                .ToListAsync();

            var result = _mapper.Map<List<InvoiceDto>>(invoices);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Purchasing")]
        public async Task<ActionResult<InvoiceDto>> GetInvoice(int id)
        {
            var invoice = await _context.Invoices
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
            var order = await _context.Orders.FindAsync(dto.OrderId);

            if (order == null)
                return NotFound();

            if (order.Status != OrderStatus.Pending)
                return Conflict(new
                {
                    code = "CLOSED_OR_REJECTED_ORDER",
                    message = "Cannot create an invoice for a closed or rejected order."
                });

            if (await _context.DeliveryNotes.AnyAsync(dn =>
                dn.CarrierId == dto.CarrierId &&
                dn.OrderId == dto.OrderId &&
                dn.Status == DeliveryNoteStatus.Pending))
            {
                return Conflict(new
                {
                    code = "CARRIER_HAS_PENDING_DELIVERY_NOTES",
                    message = "Cannot create an invoice: the carrier has pending delivery notes for this order."
                });
            }

            if (await _context.Invoices.AnyAsync(i => i.InvoiceNumber == dto.InvoiceNumber))
                return Conflict(new
                {
                    code = "INVOICE_NUMBER_ALREADY_EXISTS",
                    message = "Invoice number already exists."
                });

            if (await _context.Invoices.AnyAsync(i => i.CarrierId == dto.CarrierId && i.OrderId == dto.OrderId))
                return Conflict(new
                {
                    code = "INVOICE_CARRIER_ORDER_ALREADY_EXISTS",
                    message = "An invoice for this carrier and order already exists."
                });

            var invoice = _mapper.Map<Invoice>(dto);
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Purchasing")]
        public async Task<IActionResult> UpdateInvoice(int id, [FromBody] UpdateInvoiceDto dto)
        {
            if (await _context.Invoices.AnyAsync(i => i.InvoiceNumber == dto.InvoiceNumber && i.Id != id))
                return Conflict(new
                {
                    code = "INVOICE_NUMBER_ALREADY_EXISTS",
                    message = "Invoice number already exists."
                });

            if (await _context.Invoices.AnyAsync(i => i.CarrierId == dto.CarrierId && i.OrderId == dto.OrderId && i.Id != id))
                return Conflict(new
                {
                    code = "INVOICE_CARRIER_ORDER_ALREADY_EXISTS",
                    message = "An invoice for this carrier and order already exists."
                });

            if (await _context.DeliveryNotes.AnyAsync(dn =>
                dn.CarrierId == dto.CarrierId &&
                dn.OrderId == dto.OrderId &&
                dn.Status == DeliveryNoteStatus.Pending))
            {
                return Conflict(new
                {
                    code = "CARRIER_HAS_PENDING_DELIVERY_NOTES",
                    message = "Cannot create an invoice: the carrier has pending delivery notes for this order."
                });
            }

            var existingInvoice = await _context.Invoices
                .Include(i => i.Order)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (existingInvoice == null)
                return NotFound();

            if (existingInvoice.Order.Status != OrderStatus.Pending)
            {
                return Conflict(new
                {
                    code = "CLOSED_OR_REJECTED_ORDER",
                    message = "Cannot edit invoice from a closed or rejected order."
                });
            }

            _mapper.Map(dto, existingInvoice);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, Purchasing")]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Order)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (invoice == null)
                return NotFound();

            if (invoice.Order.Status != OrderStatus.Pending)
            {
                return Conflict(new
                {
                    code = "CLOSED_OR_REJECTED_ORDER",
                    message = "Cannot delete invoice from a closed order."
                });
            }
            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("bulk")]
        [Authorize(Roles = "Admin, Purchasing")]
        public async Task<IActionResult> DeleteInvoicesBulk([FromBody] DeleteEntitiesDto dto)
        {
            var invoices = await _context.Invoices
                .Where(i => dto.Ids.Contains(i.Id))
                .Include(i => i.Order)
                .ToListAsync();

            if (invoices.Count == 0)
                return NotFound();

            if (invoices.Any(i => i.Order.Status == OrderStatus.Closed || i.Order.Status == OrderStatus.Rejected))
            {
                return Conflict(new
                {
                    code = "CLOSED_OR_REJECTED_ORDER",
                    message = "Cannot delete invoices because at least one is associated with a closed order."
                });
            }

            _context.Invoices.RemoveRange(invoices);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
