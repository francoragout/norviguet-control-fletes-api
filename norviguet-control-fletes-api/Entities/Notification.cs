namespace norviguet_control_fletes_api.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; } = false;
        public string Link { get; set; } = string.Empty;

        // Relación con User
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
