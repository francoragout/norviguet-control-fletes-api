using norviguet_control_fletes_api.Models.DTOs.Common;
using norviguet_control_fletes_api.Models.DTOs.Customer;

namespace norviguet_control_fletes_api.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<PagedResultDto<CustomerDto>> GetAllAsync(PagedRequestDto dto, CancellationToken cancellationToken);
        Task<CustomerDto> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<CustomerDto> CreateAsync(CustomerCreateDto dto, CancellationToken cancellationToken);
        Task<CustomerDto> UpdateAsync(int id, CustomerUpdateDto dto, CancellationToken cancellationToken);
        Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken);
    }
}