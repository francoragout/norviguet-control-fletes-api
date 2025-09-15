namespace norviguet_control_fletes_api.Models.Notification
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string Link { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}
