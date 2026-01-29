using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Common.Middlewares;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.Account;
using norviguet_control_fletes_api.Services.Interfaces;
using BCrypt.Net;

namespace norviguet_control_fletes_api.Services
{
    public class AccountService(ApplicationDbContext context, IMapper mapper) : IAccountService
    {
        public async Task<AccountDto> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var user = await context.Users
                .AsNoTracking()
                .Where(x => x.Id == id)
                .ProjectTo<AccountDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException("Account not found.");
            return user;
        }

        public async Task<AccountDto> UpdateNameAsync(int id, AccountNameUpdateDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var user = await context.Users
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new NotFoundException("Account not found.");
            user.Name = dto.Name;
            await context.SaveChangesAsync(cancellationToken);
            return mapper.Map<AccountDto>(user);

        }

        public async Task<AccountDto> UpdatePasswordAsync(int id, AccountPasswordUpdateDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var user = await context.Users
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new NotFoundException("Account not found.");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            {
                throw new UnauthorizedException("Current password is incorrect.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await context.SaveChangesAsync(cancellationToken);
            
            return mapper.Map<AccountDto>(user);
        }
    }
}
