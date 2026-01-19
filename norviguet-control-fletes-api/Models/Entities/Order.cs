using norviguet_control_fletes_api.Models.Enums;

namespace norviguet_control_fletes_api.Models.Entities
{
    public class Order : AuditableEntity
    {
        public string OrderNumber { get; set; } = string.Empty;
        public OrderIncoterm Incoterm { get; set; } = OrderIncoterm.CIF;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

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
