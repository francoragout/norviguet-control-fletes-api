namespace norviguet_control_fletes_api.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        //public DateTime CreatedAt { get; set; }
        public int PointOfSale { get; set; }

        // Relación uno a muchos con Order
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
