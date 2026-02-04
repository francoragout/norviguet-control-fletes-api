using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.DTOs.PaymentOrder
{
    public class PaymentOrderCreateDto
    {
        [Required]
        public string Number { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public int CarrierId { get; set; }
    }
}