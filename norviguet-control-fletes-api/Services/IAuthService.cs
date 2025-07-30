using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models;

namespace norviguet_control_fletes_api.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDto request);
        Task<TokenResponseDto?> LoginAsync(UserDto request);
        Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request);
    }
}
