using norviguet_control_fletes_api.Models.DTOs.Customer;

namespace norviguet_control_fletes_api.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<IReadOnlyList<CustomerDto>> GetAllAsync();
        Task<CustomerDto> GetByIdAsync(int id);
        Task<CustomerDto> CreateAsync(CustomerCreateDto dto);
        Task<CustomerDto> UpdateAsync(int id, CustomerUpdateDto dto);
        Task DeleteAsync(int id);
        Task DeleteBulkAsync(IEnumerable<int> ids);
    }
}