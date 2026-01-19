using norviguet_control_fletes_api.Models.Enums;

namespace norviguet_control_fletes_api.Models.DTOs.User
{
    public class UserDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Pending;
        public string? ImageUrl { get; set; }
    }
}
