using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Carrier;
using norviguet_control_fletes_api.Models.Common;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CarrierController : ControllerBase
    {
        private readonly NorviguetDbContext _context;
        private readonly IMapper _mapper;

        public CarrierController(NorviguetDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<CarrierDto>>> GetCarriers()
        {
            var carriers = await _context.Carriers.ToListAsync();
            var result = _mapper.Map<List<CarrierDto>>(carriers);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<ActionResult<CarrierDto>> GetCarrier(int id)
        {
            var carrier = await _context.Carriers.FindAsync(id);
            if (carrier == null)
                return NotFound();
            var result = _mapper.Map<CarrierDto>(carrier);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<IActionResult> CreateCarrier([FromBody] CreateCarrierDto dto)
        {
            var carrier = _mapper.Map<Carrier>(dto);
            _context.Carriers.Add(carrier);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<IActionResult> UpdateCarrier(int id, [FromBody] UpdateCarrierDto dto)
        {
            var carrier = await _context.Carriers.FindAsync(id);
            if (carrier == null)
                return NotFound();
            _mapper.Map(dto, carrier);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<IActionResult> DeleteCarrier(int id)
        {
            var carrier = await _context.Carriers
                .Include(c => c.DeliveryNotes)
                .Include(c => c.PaymentOrders)
                .Include(c => c.Invoices)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (carrier == null)
                return NotFound();

            if ((carrier.DeliveryNotes?.Any() == true) ||
                (carrier.PaymentOrders?.Any() == true) ||
                (carrier.Invoices?.Any() == true))
            {
                return Conflict(new 
                {
                    code = "CANNOT_DELETE_CARRIER_WITH_ASSOCIATED_RECORDS",
                    message = "Carrier cannot be deleted because it has associated delivery notes, payment orders, or invoices."
                });
            }

            _context.Carriers.Remove(carrier);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("bulk")]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<IActionResult> DeleteCarriersBulk([FromBody] DeleteEntitiesDto dto)
        {
            var carriers = await _context.Carriers
                .Where(c => dto.Ids.Contains(c.Id))
                .Include(c => c.DeliveryNotes)
                .Include(c => c.PaymentOrders)
                .Include(c => c.Invoices)
                .ToListAsync();

            if (carriers.Count == 0)
                return NotFound();

            var cannotDelete = carriers
                .Where(c => (c.DeliveryNotes?.Any() == true) ||
                             (c.PaymentOrders?.Any() == true) ||
                             (c.Invoices?.Any() == true))
                .ToList();

            var canDelete = carriers.Except(cannotDelete).ToList();

            if (canDelete.Any())
            {
                _context.Carriers.RemoveRange(canDelete);
                await _context.SaveChangesAsync();
            }

            if (cannotDelete.Any())
            {
                return Conflict(new
                {
                    code = "CANNOT_DELETE_CARRIERS_WITH_ASSOCIATED_RECORDS",
                    message = "Some carriers could not be deleted because they have associated delivery notes, payment orders, or invoices.",
                });
            }

            return NoContent();
        }

        [HttpGet("by-order/{orderId}/with-delivery-notes")]
        [Authorize(Roles = "Admin, Purchasing")]
        public async Task<ActionResult<List<CarrierDto>>> GetCarriersWithDeliveryNotesByOrder(int orderId)
        {
            var carriers = await _context.DeliveryNotes
                .Where(dn => dn.OrderId == orderId)
                .Select(dn => dn.Carrier)
                .Distinct()
                .ToListAsync();

            var result = _mapper.Map<List<CarrierDto>>(carriers);
            return Ok(result);
        }

        [HttpGet("by-order/{orderId}/with-invoices")]
        [Authorize(Roles = "Admin, Payments")]
        public async Task<ActionResult<List<CarrierDto>>> GetCarriersWithInvoicesByOrder(int orderId)
        {
            var carriers = await _context.Invoices
                .Where(i => i.OrderId == orderId)
                .Select(i => i.Carrier)
                .Distinct()
                .ToListAsync();
            var result = _mapper.Map<List<CarrierDto>>(carriers);
            return Ok(result);
        }
    }
}
