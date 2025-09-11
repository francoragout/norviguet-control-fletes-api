namespace norviguet_control_fletes_api.Models.Permission
{
    public class UpdatePermissionDto
    {
        public int UserId { get; set; }
        public string Route { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
    }
}
