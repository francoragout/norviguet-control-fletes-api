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

            if (order.Status == OrderStatus.Closed || order.Status == OrderStatus.Rejected)
                return Conflict(new {
                    code = "CANNOT_CREATE_DELIVERY_NOTE_FOR_CLOSED_OR_REJECTED_ORDER",
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

            if (deliveryNote.Order.Status == OrderStatus.Closed || deliveryNote.Order.Status == OrderStatus.Rejected)
            {
                return Conflict(new
                {
                    code = "CANNOT_EDIT_CLOSED_OR_REJECTED_ORDER_DELIVERY_NOTE",
                    message = "Cannot edit delivery note from a closed or rejected order."
                });
            }

            if (deliveryNote.Status == DeliveryNoteStatus.Cancelled || deliveryNote.Status == DeliveryNoteStatus.Approved)
                return BadRequest(new
                {
                    code = "CANNOT_UPDATE_CANCELLED_OR_APPROVED_DELIVERY_NOTE",
                    message = "Cannot update a cancelled or approved delivery note."
                });

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

            if (deliveryNote.Status == DeliveryNoteStatus.Approved || deliveryNote.Status == DeliveryNoteStatus.Cancelled)
            {
                return Conflict(new
                {
                    code = "CANNOT_DELETE_APPROVED_OR_CANCELLED_DELIVERY_NOTE",
                    message = "Cannot delete an approved or cancelled delivery note."
                });
            }

            if (deliveryNote.Order.Status == OrderStatus.Closed)
            {
                return Conflict(new
                {
                    code = "CANNOT_DELETE_CLOSED_ORDER_DELIVERY_NOTE",
                    message = "Cannot delete delivery note from a closed order."
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

            if (deliveryNotes.Any(dn => dn.Order.Status == OrderStatus.Closed))
            {
                return Conflict(new
                {
                    code = "CANNOT_DELETE_CLOSED_ORDER_DELIVERY_NOTES",
                    message = "Cannot delete delivery notes because at least one is associated with a closed order."
                });
            }

            if (deliveryNotes.Any(dn => dn.Status == DeliveryNoteStatus.Approved || dn.Status == DeliveryNoteStatus.Cancelled))
            {
                return Conflict(new
                {
                    code = "CANNOT_DELETE_APPROVED_OR_CANCELLED_DELIVERY_NOTES",
                    message = "Cannot delete delivery notes because at least one is approved or cancelled."
                });
            }

            _context.DeliveryNotes.RemoveRange(deliveryNotes);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
