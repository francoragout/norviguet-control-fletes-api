using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.Location
{
    public class UpdateLocationDto
    {
        [Required]
        [StringLength(50, ErrorMessage = "The name must be between 1 and 50 characters long.")]
        public string Name { get; set; } = string.Empty;
        [Required]
        [StringLength(50, ErrorMessage = "The address must be between 1 and 50 characters long.")]
        public string Address { get; set; } = string.Empty;
        [Required]
        [StringLength(50, ErrorMessage = "The city must be between 1 and 50 characters long.")]
        public string City { get; set; } = string.Empty;
        [Required]
        public int CustomerId { get; set; }
    }
}