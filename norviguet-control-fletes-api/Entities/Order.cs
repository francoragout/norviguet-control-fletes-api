namespace norviguet_control_fletes_api.Entities
{
    public enum OrderStatus
    {
        Pending,
        Rejected,
        Closed,
    }

    public enum OrderIncoterm
    {
        FOB,
        CIF
    }

    public class Order
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public string OrderNumber { get; set; } = string.Empty;
        public OrderIncoterm Incoterm { get; set; } = OrderIncoterm.CIF;

        // Foreign Keys
        public int SellerId { get; set; }
        public int CustomerId { get; set; }

        // Relationships
        public required Seller Seller { get; set; }
        public required Customer Customer { get; set; }
        public ICollection<DeliveryNote> DeliveryNotes { get; set; } = new List<DeliveryNote>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public ICollection<PaymentOrder> PaymentOrders { get; set; } = new List<PaymentOrder>();
    }
}
