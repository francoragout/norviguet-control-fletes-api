using AutoMapper;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Common.Middlewares;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.Customer;
using norviguet_control_fletes_api.Models.Entities;
using norviguet_control_fletes_api.Services.Interfaces;

namespace norviguet_control_fletes_api.Services
{
    public class CustomerService(ApplicationDbContext context, IMapper mapper) : ICustomerService
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
                ?? throw new NotFoundException("Customer not found");
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
                ?? throw new NotFoundException("Customer not found");
            mapper.Map(dto, customer);
            await context.SaveChangesAsync();
            return mapper.Map<CustomerDto>(customer);
        }

        public async Task DeleteAsync(int id)
        {
            var customer = await context.Customers
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new KeyNotFoundException("Customer not found");
            var hasOrders = await context.Orders
                .AnyAsync(x => x.CustomerId == id);
            if (hasOrders)
                throw new ConflictException("Cannot delete customer because it is associated with one or more orders");
            context.Customers.Remove(customer);
            await context.SaveChangesAsync();
        }

        public async Task DeleteBulkAsync(IEnumerable<int> ids)
        {
            var customerIds = ids.ToList();
            var hasOrders = await context.Orders
                .AnyAsync(x => customerIds.Contains(x.CustomerId));
            if (hasOrders)
                throw new ConflictException("Cannot delete customers because one or more are associated with orders");
            await context.Customers
                .Where(x => customerIds.Contains(x.Id))
                .ExecuteDeleteAsync();
        }
    }
}
