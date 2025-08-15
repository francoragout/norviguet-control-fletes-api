namespace norviguet_control_fletes_api.Entities
{
    public class OrderStepConfiguration
    {
        public int Id { get; set; }
        public int Step { get; set; } // Example: 1, 2, 3...
        public string Field { get; set; } = string.Empty; // Example: "Price", "CarrierId"
        public UserRole Role { get; set; }
        public string Action { get; set; } = string.Empty; // Example: "Edit", "View"
    }
}