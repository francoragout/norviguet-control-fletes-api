using AutoMapper;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.Customer;
using norviguet_control_fletes_api.Models.Entities;

namespace norviguet_control_fletes_api.Services
{
    public class CustomerService(ApplicationDbContext context, IMapper mapper)
    {
        public async Task<IReadOnlyList<CustomerDto>> GetAllAsync()
        {
            var customers = await context.Customers
                .AsNoTracking()
                .ToListAsync();
            return mapper.Map<IReadOnlyList<CustomerDto>>(customers);
        }

        public async Task<CustomerDto> GetByIdAsync(int id)
        {
            var customer = await context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new KeyNotFoundException("Customer not found");
            return mapper.Map<CustomerDto>(customer);
        }

        public async Task<CustomerDto> CreateAsync(CustomerCreateDto dto)
        {
            var customer = mapper.Map<Customer>(dto);
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
            return mapper.Map<CustomerDto>(customer);
        }

        public async Task<CustomerDto> UpdateAsync(int id, CustomerUpdateDto dto)
        {
            var customer = await context.Customers
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new KeyNotFoundException("Customer not found");
            mapper.Map(dto, customer);
            await context.SaveChangesAsync();
            return mapper.Map<CustomerDto>(customer);
        }

        public async Task DeleteAsync(int id)
        {
            var customer = await context.Customers
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new KeyNotFoundException("Customer not found");
            context.Customers.Remove(customer);
            await context.SaveChangesAsync();
        }

        public async Task DeleteBulkAsync(IEnumerable<int> ids)
        {
            await context.Customers
                .Where(x => ids.Contains(x.Id))
                .ExecuteDeleteAsync();
        }
    }
}
