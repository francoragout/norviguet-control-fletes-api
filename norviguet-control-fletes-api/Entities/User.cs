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
    public enum UserStatus
    {
        Active,
        Inactive,
        Suspended
    }
    public class User
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Pending;
        public UserStatus Status { get; set; } = UserStatus.Inactive;

        // Relacion uno a muchos con RefreshToken
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
