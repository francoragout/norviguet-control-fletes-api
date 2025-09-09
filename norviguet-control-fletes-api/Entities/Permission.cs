namespace norviguet_control_fletes_api.Entities
{
    public class Permission
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Route { get; set; } = string.Empty;
        public List<string> AllowedMethods { get; set; } = new List<string>();

        // Relación con User
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
