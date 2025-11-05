using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.Payment
{
    public class CreatePaymentOrderDto
    {
        [Required]
        public string PaymentOrderNumber { get; set; } = string.Empty;
        [Required]
        public int InvoiceId { get; set; }
        [Required]
        public int OrderId { get; set; }
    }
}