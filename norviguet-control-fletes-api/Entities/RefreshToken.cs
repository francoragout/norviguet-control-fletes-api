namespace norviguet_control_fletes_api.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public string? CreatedByIp { get; set; } // IP desde donde se generó el token
        public DateTime? RevokedAt { get; set; } // Fecha de revocación, si aplica
        public string? RevokedByIp { get; set; } // IP desde donde se revocó el token
        public string? DeviceInfo { get; set; } // Opcional: información del dispositivo
        public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;

        // Relación con User
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
