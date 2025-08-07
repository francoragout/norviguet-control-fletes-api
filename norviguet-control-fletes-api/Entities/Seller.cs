namespace norviguet_control_fletes_api.Entities
{
    public class Seller
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Relación uno a muchos con Order
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
