using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.DTOs.Customer
{
    public class CreateCustomerDto
    {
        [Required]
        [StringLength(50, ErrorMessage = "The name must be between 1 and 50 characters long.")]
        public string Name { get; set; } = string.Empty;
        [Required]
        [RegularExpression(@"^\d{2}-\d{8}-\d{1}$", ErrorMessage = "The CUIT must be in the format NN-NNNNNNNN-N.")]
        public string CUIT { get; set; } = string.Empty;
        [StringLength(50, ErrorMessage = "The business name must be between 1 and 50 characters long.")]
        public string? BusinessName { get; set; }
        public string? Email { get; set; }
    }
}
