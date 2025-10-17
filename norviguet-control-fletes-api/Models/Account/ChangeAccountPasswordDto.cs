using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.Account
{
    public class ChangeAccountPasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [MaxLength(30, ErrorMessage = "Password cannot exceed 30 characters.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=[\]{};':""\\|,.<>\/?]).+$", ErrorMessage = "Password must contain at least one uppercase letter, one number, and one special character.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords doesn't match.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
