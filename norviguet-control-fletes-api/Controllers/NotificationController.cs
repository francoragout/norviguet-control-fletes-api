using AutoMapper;
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

        // 1. Obtener notificaciones por usuario
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<NotificationDto>>> GetNotificationsByUser(int userId)
        {
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

        // 2. Marcar todas las notificaciones del usuario como leídas
        [HttpPatch("mark-all-as-read/{userId}")]
        public async Task<IActionResult> MarkAllAsRead(int userId)
        {
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

        // 3. Crear notificación para usuario
        [HttpPost]
        public async Task<ActionResult<NotificationDto>> CreateNotification([FromBody] CreateNotificationDto dto)
        {
            var notification = await _notificationService.CreateNotificationAsync(dto);
            var resultDto = _mapper.Map<NotificationDto>(notification);
            return CreatedAtAction(nameof(GetNotificationsByUser), new { userId = notification.UserId }, resultDto);
        }
    }
}
