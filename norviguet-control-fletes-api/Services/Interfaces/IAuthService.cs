using norviguet_control_fletes_api.Models.DTOs.Auth;
using norviguet_control_fletes_api.Models.DTOs.User;

namespace norviguet_control_fletes_api.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginDto loginDto, HttpContext httpContext, CancellationToken cancellationToken);
        Task<UserDto> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken);
        Task<LoginResponseDto> RefreshTokenAsync(HttpContext httpContext, CancellationToken cancellationToken);
        Task RevokeTokenAsync(HttpContext httpContext, CancellationToken cancellationToken);
    }
}