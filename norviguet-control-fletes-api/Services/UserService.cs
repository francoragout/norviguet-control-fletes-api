using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Common.Middlewares;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.User;
using norviguet_control_fletes_api.Models.Enums;
using norviguet_control_fletes_api.Services.Interfaces;

namespace norviguet_control_fletes_api.Services
{
    public class UserService(ApplicationDbContext context, IMapper mapper) : IUserService
    {
        public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await context.Users
                .AsNoTracking()
                .OrderByDescending(u => u.CreatedAt)
                .ProjectTo<UserDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        public async Task<UserDto> UpdateRoleAsync(int id, UserRoleUpdateDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
                ?? throw new NotFoundException("User not found");

            if (!Enum.TryParse<UserRole>(dto.Role, true, out var role) ||
                !Enum.IsDefined(typeof(UserRole), role))
            {
                throw new ArgumentException($"Invalid role: {dto.Role}");
            }

            user.Role = role;
            await context.SaveChangesAsync(cancellationToken);
            return mapper.Map<UserDto>(user);
        }

        public async Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(ids);

            var idList = ids.Distinct().ToList();
            if (idList.Count == 0) return;

            var existingCount = await context.Users
                .CountAsync(u => idList.Contains(u.Id), cancellationToken);

            if (existingCount != idList.Count)
                throw new NotFoundException("Some users were not found.");

            await context.Users
                .Where(u => idList.Contains(u.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
