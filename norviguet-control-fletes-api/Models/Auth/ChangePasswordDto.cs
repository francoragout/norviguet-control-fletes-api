using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.Auth
{
    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        [MaxLength(30, ErrorMessage = "Password cannot exceed 30 characters.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords doesn't match.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
