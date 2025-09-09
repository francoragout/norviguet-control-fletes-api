namespace norviguet_control_fletes_api.Models.Permission
{
    public class CreatePermissionDto
    {
        public int UserId { get; set; }
        public string Route { get; set; } = string.Empty;
        public List<string> AllowedMethods { get; set; } = new List<string>();
    }
}
