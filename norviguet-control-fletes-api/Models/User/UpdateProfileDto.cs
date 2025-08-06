using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.User
{
    public class UpdateProfileDto
    {
        [Required]
        [MinLength(2, ErrorMessage = "First name must be at least 2 characters long.")]
        [MaxLength(30, ErrorMessage = "First name cannot exceed 30 characters.")]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        [MinLength(2, ErrorMessage = "Last name must be at least 2 characters long.")]
        [MaxLength(30, ErrorMessage = "Last name cannot exceed 30 characters.")]
        public string LastName { get; set; } = string.Empty;
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}
