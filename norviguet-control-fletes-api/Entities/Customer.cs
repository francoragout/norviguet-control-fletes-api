namespace norviguet_control_fletes_api.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? BusinessName { get; set; }
        public string? CUIT { get; set; }
        public string? Email { get; set; }

        // Relationships
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
