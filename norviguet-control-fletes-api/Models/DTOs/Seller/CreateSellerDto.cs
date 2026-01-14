using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.DTOs.Seller
{
    public class CreateSellerDto
    {
        [Required]
        [StringLength(50, ErrorMessage = "Name must be between 1 and 50 characters long.")]
        public string Name { get; set; } = string.Empty;
        public string? Zone { get; set; }
    }
}