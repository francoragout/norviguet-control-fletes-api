namespace norviguet_control_fletes_api.Entities
{
    public enum PaymentPOS
    {
        POS_0 = 0,
        POS_99 = 99,
    }

    public class Payment
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public PaymentPOS POS { get; set; }

        // Relación uno a muchos con Order
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
