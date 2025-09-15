using AutoMapper;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Notification;
using Microsoft.EntityFrameworkCore;

namespace norviguet_control_fletes_api.Services
{
    public class NotificationService : INotificationService
    {
        private readonly NorviguetDbContext _context;
        private readonly IMapper _mapper;

        public NotificationService(NorviguetDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Notification> CreateNotificationAsync(CreateNotificationDto dto)
        {
            var notification = _mapper.Map<Notification>(dto);
            notification.CreatedAt = DateTime.UtcNow;
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }
    }
}