using System.ComponentModel.DataAnnotations;
using norviguet_control_fletes_api.Entities;

namespace norviguet_control_fletes_api.Models.Order
{
    public class CreateOrderDto
    {
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        [Required]
        public required string DeliveryNote { get; set; }
        public int? CarrierId { get; set; }
        [Required]
        public required int SellerId { get; set; }
        [Required]
        public required int CustomerId { get; set; }
        [Required]
        public decimal Price { get; set; }
        public int? InvoiceId { get; set; }
        public int? PaymentId { get; set; }
        [Required]
        public required float DiscountRate { get; set; }
    }
}