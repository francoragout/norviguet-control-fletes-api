using norviguet_control_fletes_api.Models.Notification;
using norviguet_control_fletes_api.Entities;
using System.Threading.Tasks;

namespace norviguet_control_fletes_api.Services
{
    public interface INotificationService
    {
        Task<Notification> CreateNotificationAsync(CreateNotificationDto dto);
    }
}