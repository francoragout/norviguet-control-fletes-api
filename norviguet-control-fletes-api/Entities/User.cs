namespace norviguet_control_fletes_api.Entities
{
    public enum UserRole
    {
        Pending,
        Admin,
        Logistics,
        Purchasing,
        Payments
    }

    public class User
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Pending;
        public byte[]? Image { get; set; }

        // Relacion uno a muchos
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
