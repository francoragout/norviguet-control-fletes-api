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
        [Authorize(Roles = "Admin, Logistics")]
        public async Task<ActionResult<List<DeliveryNoteDto>>> GetDeliveryNotesByOrderId(int orderId)
        {
            var deliveryNotes = await _context.DeliveryNotes
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
                .Include(d => d.Carrier)
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
            var deliveryNote = _mapper.Map<DeliveryNote>(dto);
            _context.DeliveryNotes.Add(deliveryNote);
            await _context.SaveChangesAsync();

            // Send notification to admin and logistics users
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
            var deliveryNote = await _context.DeliveryNotes.FindAsync(id);
            if (deliveryNote == null) 
                return NotFound();

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
            var deliveryNote = await _context.DeliveryNotes.FindAsync(id);
            if (deliveryNote == null) 
                return NotFound();
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
                .ToListAsync();
            if (deliveryNotes.Count ==0)
                return NotFound();
            _context.DeliveryNotes.RemoveRange(deliveryNotes);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
