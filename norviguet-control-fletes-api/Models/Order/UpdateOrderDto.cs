using norviguet_control_fletes_api.Entities;

namespace norviguet_control_fletes_api.Models.Order
{
    public class UpdateOrderDto
    {
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public string? DeliveryNote { get; set; }
        public int? CarrierId { get; set; }
        public int? SellerId { get; set; }
        public int? CustomerId { get; set; }
        public decimal? Price { get; set; }
        public int? InvoiceId { get; set; }
        public int? PaymentId { get; set; }
        public float? DiscountRate { get; set; }
    }
}