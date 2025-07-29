namespace norviguet_control_fletes_api.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<Location> Locations { get; set; } = new List<Location>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
