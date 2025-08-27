using norviguet_control_fletes_api.Entities;

namespace norviguet_control_fletes_api.Models.Order
{
    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public OrderStatus Status { get; set; }
        public string? DeliveryNote { get; set; }
        public string? CarrierId { get; set; }
        public string? CarrierName { get; set; }
        public int SellerId { get; set; }
        public int CustomerId { get; set; }
        public decimal Price { get; set; }
        public int? PaymentId { get; set; }
        public int? InvoiceId { get; set; }
        public float DiscountRate { get; set; }
    }
}