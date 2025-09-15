namespace norviguet_control_fletes_api.Models.Notification
{
    public class CreateNotificationDto
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public string Link { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}
