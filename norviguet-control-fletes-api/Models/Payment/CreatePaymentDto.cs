using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.Payment
{
    public class CreatePaymentDto
    {
        [Required]
        [StringLength(50, ErrorMessage = "The PointOfSale must be between 1 and 50 characters long.")]
        public string PointOfSale { get; set; } = string.Empty;
    }
}