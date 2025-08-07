namespace norviguet_control_fletes_api.Entities
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;

        // Relación con Customer
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

    }
}
