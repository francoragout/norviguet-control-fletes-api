using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Common;
using norviguet_control_fletes_api.Models.DeliveryNote;
using norviguet_control_fletes_api.Models.Notification;
using norviguet_control_fletes_api.Services;

namespace norviguet_control_fletes_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DeliveryNoteController : ControllerBase
    {
        private readonly NorviguetDbContext _context;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public DeliveryNoteController(NorviguetDbContext context, IMapper mapper, INotificationService notificationService)
        {
            _context = context;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<List<DeliveryNoteDto>>> GetDeliveryNotesByOrderId(int orderId)
        {
            var deliveryNotes = await _context.DeliveryNotes
                .Where(dn => dn.OrderId == orderId)
                .Include(dn => dn.Order)
                .Include(dn => dn.Carrier)
                .ToListAsync();
            var result = _mapper.Map<List<DeliveryNoteDto>>(deliveryNotes);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<ActionResult<DeliveryNoteDto>> GetDeliveryNote(int id)
        {
            var note = await _context.DeliveryNotes
                .FirstOrDefaultAsync(d => d.Id == id);
            if (note == null)
                return NotFound();
            var result = _mapper.Map<DeliveryNoteDto>(note);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<ActionResult<DeliveryNoteDto>> CreateDeliveryNote(CreateDeliveryNoteDto dto)
        {
            var order = await _context.Orders.FindAsync(dto.OrderId);
            if (order == null)
                return NotFound();

            if (order.Status != OrderStatus.Pending)
                return Conflict(new
                {
                    code = "CLOSED_OR_REJECTED_ORDER",
                    message = "Cannot create delivery note for a closed or rejected order."
                });

            if (await _context.DeliveryNotes.AnyAsync(dn => dn.DeliveryNoteNumber == dto.DeliveryNoteNumber))
                return Conflict(new
                {
                    code = "DELIVERY_NOTE_NUMBER_ALREADY_EXISTS",
                    message = "Delivery note number already exists."
                });

            var deliveryNote = _mapper.Map<DeliveryNote>(dto);
            _context.DeliveryNotes.Add(deliveryNote);
            await _context.SaveChangesAsync();

            var userToNotify = await _context.Users
                .Where(u => u.Role == UserRole.Admin || u.Role == UserRole.Logistics)
                .ToListAsync();

            foreach (var user in userToNotify)
            {
                var notificationDto = new CreateNotificationDto
                {
                    UserId = user.Id,
                    Title = "Nuevo Remito",
                    Message = $"Se ha creado un nuevo remito (ID: {deliveryNote.Id}).",
                    Link = $"/dashboard/orders/{deliveryNote.OrderId}/delivery-notes"
                };
                await _notificationService.CreateNotificationAsync(notificationDto);
            }

            var result = _mapper.Map<DeliveryNoteDto>(deliveryNote);
            return CreatedAtAction(nameof(GetDeliveryNote), new { id = deliveryNote.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<IActionResult> UpdateDeliveryNote(int id, [FromBody] UpdateDeliveryNoteDto dto)
        {
            var deliveryNote = await _context.DeliveryNotes
                .Include(dn => dn.Order)
                .FirstOrDefaultAsync(dn => dn.Id == id);

            if (deliveryNote == null)
                return NotFound();

            if (deliveryNote.Order.Status != OrderStatus.Pending || deliveryNote.Status != DeliveryNoteStatus.Pending)
            {
                return Conflict(new
                {
                    code = "APPROVED_OR_REJECTED_DELIVERY_NOTE_CLOSED_OR_REJECTED_ORDER",
                    message = "Cannot edit delivery note that is Approved or Rejected, or related to a closed or rejected order."
                });
            }

            if (await _context.DeliveryNotes.AnyAsync(dn => dn.DeliveryNoteNumber == dto.DeliveryNoteNumber && dn.Id != id))
                return Conflict(new
                {
                    code = "DELIVERY_NOTE_NUMBER_ALREADY_EXISTS",
                    message = "Delivery note number already exists."
                });

            _mapper.Map(dto, deliveryNote);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDeliveryNoteStatus(int id, [FromBody] UpdateDeliveryNoteStatusDto dto)
        {
            var deliveryNote = await _context.DeliveryNotes.FindAsync(id);

            if (deliveryNote == null)
                return NotFound();

            _mapper.Map(dto, deliveryNote);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDeliveryNote(int id)
        {
            var deliveryNote = await _context.DeliveryNotes
                .Include(dn => dn.Order)
                .FirstOrDefaultAsync(dn => dn.Id == id);

            if (deliveryNote == null)
                return NotFound();

            // Solo se puede eliminar si el remito y la orden están en estado Pending
            if (deliveryNote.Status != DeliveryNoteStatus.Pending || deliveryNote.Order.Status != OrderStatus.Pending)
            {
                return Conflict(new
                {
                    code = "APPROVED_OR_REJECTED_DELIVERY_NOTE_CLOSED_OR_REJECTED_ORDER",
                    message = "Only delivery notes with Pending status or related to orders with Pending status can be deleted."
                });
            }

            _context.DeliveryNotes.Remove(deliveryNote);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("bulk")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDeliveryNotesBulk([FromBody] DeleteEntitiesDto dto)
        {
            var deliveryNotes = await _context.DeliveryNotes
                .Where(dn => dto.Ids.Contains(dn.Id))
                .Include(dn => dn.Order)
                .ToListAsync();

            if (deliveryNotes.Count == 0)
                return NotFound();

            // Filtrar los que se pueden eliminar: pendientes y orden pendiente
            var notesToDelete = deliveryNotes
                .Where(dn => dn.Status == DeliveryNoteStatus.Pending && dn.Order.Status == OrderStatus.Pending)
                .ToList();

            // Si hay remitos que no cumplen la condición, lanzar error pero eliminar los válidos
            if (notesToDelete.Count < deliveryNotes.Count)
            {
                if (notesToDelete.Count > 0)
                {
                    _context.DeliveryNotes.RemoveRange(notesToDelete);
                    await _context.SaveChangesAsync();
                }

                return Conflict(new
                {
                    code = "APPROVED_OR_REJECTED_DELIVERY_NOTE_CLOSED_OR_REJECTED_ORDER",
                    message = "Only delivery notes with Pending status or related to orders with Pending status can be deleted. Others were not deleted."
                });
            }

            _context.DeliveryNotes.RemoveRange(notesToDelete);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
