using norviguet_control_fletes_api.Models.DTOs.Account;

namespace norviguet_control_fletes_api.Services.Interfaces
{
    public interface IAccountService
    {
        Task<AccountDto> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<AccountDto> UpdateNameAsync(int id, AccountNameUpdateDto dto, CancellationToken cancellationToken);
        Task<AccountDto> UpdatePasswordAsync(int id, string newEmail, CancellationToken cancellationToken);
    }
}