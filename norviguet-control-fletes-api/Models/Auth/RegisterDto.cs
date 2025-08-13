using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.Auth
{
    public class RegisterDto
    {
        [Required]
        [StringLength(50, ErrorMessage = "The name must be between 1 and 50 characters long.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s'-]+$", ErrorMessage = "Name can only contain letters and spaces.")]
        public string Name { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        [MaxLength(30, ErrorMessage = "Password cannot exceed 30 characters.")]
        public string Password { get; set; } = string.Empty;
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
