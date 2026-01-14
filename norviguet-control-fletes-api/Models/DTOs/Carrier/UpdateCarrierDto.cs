using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.DTOs.Carrier
{
    public class UpdateCarrierDto
    {
        [Required]
        [StringLength(50, ErrorMessage = "The name must be between 1 and 50 characters long.")]
        public string Name { get; set; } = string.Empty;
    }
}
