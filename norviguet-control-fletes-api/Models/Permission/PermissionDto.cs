namespace norviguet_control_fletes_api.Models.Permission
{
    public class PermissionDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
    }
}
