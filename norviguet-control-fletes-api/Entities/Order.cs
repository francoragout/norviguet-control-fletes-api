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
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public string DeliveryNote { get; set; } = string.Empty;
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public int? CarrierId { get; set; }
        public Carrier? Carrier { get; set; }
        public int SellerId { get; set; }
        public Seller Seller { get; set; } = null!;
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;
        public decimal Price { get; set; }
        public int PurchaseOrder { get; set; }
        public int? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        public int? PaymentOrderId { get; set; }
        public PaymentOrder? PaymentOrder { get; set; }
        public float DiscountRate { get; set; }
    }
}
