using norviguet_control_fletes_api.Models.Enums;

namespace norviguet_control_fletes_api.Models.DTOs.Account
{
    public class AccountDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }
}
