using norviguet_control_fletes_api.Models.DTOs.User;

namespace norviguet_control_fletes_api.Services.Interfaces
{
    public interface IUserService
    {
        Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken);
        Task<UserDto> UpdateRoleAsync(int id, UserRoleUpdateDto dto, CancellationToken cancellationToken);
        Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken);
    }
}