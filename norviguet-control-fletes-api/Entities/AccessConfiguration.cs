namespace norviguet_control_fletes_api.Entities
{
    public class AccessConfiguration
    {
        public int Id { get; set; }
        public string Route { get; set; } = string.Empty; // Example: "/api/orders"
        public string HttpMethod { get; set; } = string.Empty; // Example: "GET", "POST"
        public UserRole Role { get; set; }
        public string Action { get; set; } = string.Empty; // Example: "View", "Create", "Edit", "Delete"
    }
}