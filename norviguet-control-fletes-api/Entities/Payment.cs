namespace norviguet_control_fletes_api.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string PointOfSale { get; set; } = string.Empty;

        // Relación uno a muchos con Order
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
