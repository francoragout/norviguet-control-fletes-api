using norviguet_control_fletes_api.Models.DTOs.User;

namespace norviguet_control_fletes_api.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetAllAsync(CancellationToken cancellationToken);
        Task<UserDto> UpdateRoleAsync(int id, UserRoleUpdateDto dto, CancellationToken cancellationToken);
    }
}