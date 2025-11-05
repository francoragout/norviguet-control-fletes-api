namespace norviguet_control_fletes_api.Entities
{
    public class PaymentOrder
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string PaymentOrderNumber { get; set; } = string.Empty;

        // Foreign Keys
        public int OrderId { get; set; }
        public int CarrierId { get; set; }

        // Relationships
        public required Order Order { get; set; }
        public required Carrier Carrier { get; set; }
    }
}
