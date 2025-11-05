using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.Payment
{
    public class UpdatePaymentOrderDto
    {
        [Required]
        public string PaymentOrderNumber { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public int CarrierId { get; set; }
    }
}