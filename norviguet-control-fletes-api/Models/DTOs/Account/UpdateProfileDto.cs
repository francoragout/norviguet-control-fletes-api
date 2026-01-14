using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.DTOs.Account
{
    public class UpdateProfileDto
    {
        [Required]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters long.")]
        [MaxLength(30, ErrorMessage = "Name cannot exceed 30 characters.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s'-]+$", ErrorMessage = "Name can only contain letters and spaces.")]
        public string Name { get; set; } = string.Empty;
    }
}
