using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.Customer
{
    public class CreateCustomerDto
    {
        [Required]
        [StringLength(50, ErrorMessage = "The name must be between 1 and 50 characters long.")]
        public string name { get; set; } = string.Empty;
    }
}
