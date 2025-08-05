using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models;

namespace norviguet_control_fletes_api.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(RegisterDto request);
        Task<TokenResponseDto?> LoginAsync(LoginDto request);
        Task<TokenResponseDto?> RefreshTokensAsync(string refreshToken);
    }
}
