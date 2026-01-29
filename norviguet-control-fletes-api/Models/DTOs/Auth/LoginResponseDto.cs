namespace norviguet_control_fletes_api.Models.DTOs.Auth
{
    public class LoginResponseDto
    {
        public required string AccessToken { get; set; }
        public required DateTime AccessTokenExpiresAt { get; set; }
    }
}
