using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Common.Middlewares;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.Common;
using norviguet_control_fletes_api.Models.DTOs.Customer;
using norviguet_control_fletes_api.Models.Entities;
using norviguet_control_fletes_api.Services.Interfaces;

namespace norviguet_control_fletes_api.Services
{
    public class CustomerService(ApplicationDbContext context, IMapper mapper) : ICustomerService
    {
        public async Task<PagedResultDto<CustomerDto>> GetAllAsync(PagedRequestDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var query = context.Customers
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt);

            var totalItems = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip(dto.GetSkip())
                .Take(dto.PageSize)
                .ProjectTo<CustomerDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return new PagedResultDto<CustomerDto>
            {
                Items = items,
                Page = dto.Page,
                PageSize = dto.PageSize,
                TotalItems = totalItems
            };
        }

        public async Task<CustomerDto> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var customer = await context.Customers
                .AsNoTracking()
                .Where(x => x.Id == id)
                .ProjectTo<CustomerDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException("Customer not found");
            return customer;
        }

        public async Task<CustomerDto> CreateAsync(CustomerCreateDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var customer = mapper.Map<Customer>(dto);
            context.Customers.Add(customer);
            await context.SaveChangesAsync(cancellationToken);
            return mapper.Map<CustomerDto>(customer);
        }

        public async Task<CustomerDto> UpdateAsync(int id, CustomerUpdateDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var customer = await context.Customers
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
                ?? throw new NotFoundException("Customer not found");

            mapper.Map(dto, customer);

            try
            {
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException("The record you attempted to edit was modified by another user after you got the original value.");
            }

            return mapper.Map<CustomerDto>(customer);
        }

        public async Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(ids);

            var idList = ids.Distinct().ToList();
            if (idList.Count == 0) return;

            var existingCount = await context.Customers
                .CountAsync(c => idList.Contains(c.Id), cancellationToken);

            if (existingCount != idList.Count)
                throw new NotFoundException("Some customers were not found.");

            var conflictCount = await context.Orders
                .Where(o => idList.Contains(o.CustomerId))
                .Select(o => o.CustomerId)
                .Distinct()
                .CountAsync(cancellationToken);

            if (conflictCount > 0)
            {
                throw new ConflictException(
                    $"Operation aborted. {conflictCount} customer(s) cannot be deleted due to existing orders.");
            }

            await context.Customers
                .Where(c => idList.Contains(c.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
