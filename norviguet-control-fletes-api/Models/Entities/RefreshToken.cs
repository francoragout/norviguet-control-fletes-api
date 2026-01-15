namespace norviguet_control_fletes_api.Models.Entities
{
    public class RefreshToken : AuditableEntity
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;

        // Relationships
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
