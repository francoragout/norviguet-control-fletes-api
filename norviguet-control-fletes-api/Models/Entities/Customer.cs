namespace norviguet_control_fletes_api.Models.Entities
{
    public class Customer : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;
        public string CUIT { get; set; } = string.Empty;
        public string? BusinessName { get; set; }
        public string? Email { get; set; }

        // Relationships
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
