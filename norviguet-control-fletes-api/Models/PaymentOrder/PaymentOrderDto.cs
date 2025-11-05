using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.Payment
{
    public class PaymentOrderDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public string PaymentOrderNumber { get; set; } = string.Empty;

        // Foreign Keys
        public int OrderId { get; set; }
        public int CarrierId { get; set; }

        // Additional related data
        public string CarrierName { get; set; } = string.Empty;
    }
}
