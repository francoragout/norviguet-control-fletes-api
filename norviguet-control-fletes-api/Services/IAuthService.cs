using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Models.Auth;

namespace norviguet_control_fletes_api.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(RegisterDto request);
        Task<TokenResponseDto?> LoginAsync(LoginDto request);
        Task<TokenResponseDto?> RefreshTokensAsync(string refreshToken);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
        Task InvalidateRefreshTokenAsync(string refreshToken);
        Task<RefreshToken?> GetLatestActiveRefreshTokenAsync(User user);
        bool VerifyPassword(string password, string passwordHash);
        string HashPassword(string password);

    }
}
