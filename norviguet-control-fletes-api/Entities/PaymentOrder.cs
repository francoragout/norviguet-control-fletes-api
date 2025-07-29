namespace norviguet_control_fletes_api.Entities
{
    public class PaymentOrder
    {
        public int Id { get; set; }
        public int PointOfSale { get; set; }
        public int OrderNumber { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
