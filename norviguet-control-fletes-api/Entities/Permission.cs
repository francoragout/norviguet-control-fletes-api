namespace norviguet_control_fletes_api.Entities
{
    public class Permission
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Route { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;

        // Relación con User
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
