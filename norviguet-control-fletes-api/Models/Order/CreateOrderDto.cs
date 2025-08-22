using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.Order
{
    public class CreateOrderDto
    {
        [Required]
        public int? CarrierId { get; set; }
        [Required]
        public int? SellerId { get; set; }
        [Required]
        public int? CustomerId { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public int? PurchaseOrder { get; set; }
        public int? InvoiceId { get; set; }
        public int? PaymentOrderId { get; set; }
        [Required]
        public float DiscountRate { get; set; }
    }
}