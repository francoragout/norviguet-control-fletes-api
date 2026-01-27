using AutoMapper;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.Common;
using norviguet_control_fletes_api.Models.DTOs.Customer;
using norviguet_control_fletes_api.Services.Interfaces;

namespace norviguet_control_fletes_api.Services
{
    public class CustomerService(ApplicationDbContext context, IMapper mapper) : ICustomerService
    {
        public async Task<PagedResultDto<CustomerDto>> GetAllAsync(PagedRequestDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<CustomerDto> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<CustomerDto> CreateAsync(CustomerCreateDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<CustomerDto> UpdateAsync(int id, CustomerUpdateDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
