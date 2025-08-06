using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models
{
    public class RegisterDto
    {
        [Required]
        [MinLength(2, ErrorMessage = "First name must be at least 2 characters long.")]
        [MaxLength(30, ErrorMessage = "First name cannot exceed 30 characters.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s'-]+$", ErrorMessage = "First name can only contain letters and spaces.")]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        [MinLength(2, ErrorMessage = "Last name must be at least 2 characters long.")]
        [MaxLength(30, ErrorMessage = "Last name cannot exceed 30 characters.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s'-]+$", ErrorMessage = "Last name can only contain letters and spaces.")]
        public string LastName { get; set; } = string.Empty;
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
