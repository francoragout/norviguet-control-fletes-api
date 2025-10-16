namespace norviguet_control_fletes_api.Entities
{
    public enum OrderStatus
    {
        Pending,
        Rejected,
        Closed,
    }
    public class Order
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;      
        public string? DeliveryNote { get; set; }
        public decimal? Price { get; set; }
        public decimal? DiscountRate { get; set; }

        // Relaciones
        public int? CarrierId { get; set; }
        public Carrier? Carrier { get; set; }
        public int? SellerId { get; set; }
        public Seller? Seller { get; set; }
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public int? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        public int? PaymentId { get; set; }
        public Payment? Payment { get; set; }
    }
}
