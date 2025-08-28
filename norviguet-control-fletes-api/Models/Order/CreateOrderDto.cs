using norviguet_control_fletes_api.Entities;

namespace norviguet_control_fletes_api.Models.Order
{
    public class CreateOrderDto
    {
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public required string DeliveryNote { get; set; }
        public int? CarrierId { get; set; }
        public required int SellerId { get; set; }
        public required int CustomerId { get; set; }
        public decimal Price { get; set; }
        public int? InvoiceId { get; set; }
        public int? PaymentId { get; set; }
        public required float DiscountRate { get; set; }
    }
}