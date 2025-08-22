namespace norviguet_control_fletes_api.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;

        // Relación uno a muchos con Location y Order
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
