using AutoMapper;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.User;
using norviguet_control_fletes_api.Services.Interfaces;

namespace norviguet_control_fletes_api.Services
{
    public class UserService(ApplicationDbContext context, IMapper mapper) : IUserService
    {
        public async Task<UserDto> GetAllAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<UserDto> UpdateRoleAsync(int id, UserRoleUpdateDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
