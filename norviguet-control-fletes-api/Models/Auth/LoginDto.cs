using System.ComponentModel.DataAnnotations;

namespace norviguet_control_fletes_api.Models.Auth
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        /// <summary>
        /// Indica si el usuario desea que la sesión se recuerde (persistente).
        /// </summary>
        public bool RememberMe { get; set; } = false;
    }
}
