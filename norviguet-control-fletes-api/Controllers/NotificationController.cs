using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Notification;
using norviguet_control_fletes_api.Services;

namespace norviguet_control_fletes_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly NorviguetDbContext _context;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public NotificationController(NorviguetDbContext context, IMapper mapper, INotificationService notificationService)
        {
            _context = context;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<NotificationDto>>> GetNotifications()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);
            var cutoffDate = DateTime.UtcNow.AddDays(-30);

            // Eliminar notificaciones viejas antes de consultar
            var oldNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && n.CreatedAt < cutoffDate)
                .ToListAsync();

            if (oldNotifications.Any())
            {
                _context.Notifications.RemoveRange(oldNotifications);
                await _context.SaveChangesAsync();
            }

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && n.CreatedAt >= cutoffDate)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            var result = _mapper.Map<List<NotificationDto>>(notifications);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<NotificationDto>> CreateNotification([FromBody] CreateNotificationDto dto)
        {
            var notification = await _notificationService.CreateNotificationAsync(dto);
            var resultDto = _mapper.Map<NotificationDto>(notification);
            return CreatedAtAction(nameof(GetNotifications), new { userId = notification.UserId }, resultDto);
        }

        [Authorize]
        [HttpPatch("is-read")]
        public async Task<IActionResult> UpdateNotifications()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (!notifications.Any())
                return NotFound("No hay notificaciones no leídas para este usuario.");

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
