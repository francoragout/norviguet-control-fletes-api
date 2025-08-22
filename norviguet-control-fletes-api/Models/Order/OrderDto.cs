using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Carrier;

namespace norviguet_control_fletes_api.Models.Order
{
    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public OrderStatus Status { get; set; }
        public string? DeliveryNote { get; set; }
        public int? CarrierId { get; set; }
        public CarrierDto? Carrier { get; set; }
        public int SellerId { get; set; }
        public int CustomerId { get; set; }
        public decimal Price { get; set; }
        public int PurchaseOrder { get; set; }
        public int? PaymentOrderId { get; set; }
        public int? InvoiceId { get; set; }
        public float DiscountRate { get; set; }
    }
}