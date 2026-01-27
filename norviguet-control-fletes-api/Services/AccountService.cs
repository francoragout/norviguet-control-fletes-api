using AutoMapper;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.Account;
using norviguet_control_fletes_api.Services.Interfaces;

namespace norviguet_control_fletes_api.Services
{
    public class AccountService(ApplicationDbContext context, IMapper mapper) : IAccountService
    {
        public async Task<AccountDto> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<AccountDto> UpdateNameAsync(int id, AccountNameUpdateDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<AccountDto> UpdatePasswordAsync(int id, string newEmail, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
