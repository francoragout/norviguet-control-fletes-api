namespace norviguet_control_fletes_api.Models.Permission
{
    public class PermissionDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int UserId { get; set; }
        public string Route { get; set; } = string.Empty;
        public List<string> AllowedMethods { get; set; } = new List<string>();
    }
}
