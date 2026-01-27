using norviguet_control_fletes_api.Models.Enums;
namespace norviguet_control_fletes_api.Models.Entities
{
    public class User : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Pending;

        // Relationships
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
