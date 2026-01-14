namespace norviguet_control_fletes_api.Models.DTOs.Auth
{
    public class RefreshTokenRequestDto
    {
        public int UserId { get; set; }
        public required string RefreshToken { get; set; }
    }
}
