namespace norviguet_control_fletes_api.Models.Entities
{
    public class Seller
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Zone { get; set; }

        // Relationships
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
