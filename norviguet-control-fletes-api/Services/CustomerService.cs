using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.Common;
using norviguet_control_fletes_api.Models.DTOs.Customer;

namespace norviguet_control_fletes_api.Services
{
    public class CustomerService(ApplicationDbContext context, IMapper mapper)
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
                ?? throw new KeyNotFoundException("Customer not found");
            return customer;
        }

        public async Task<CustomerDto> CreateAsync(CustomerCreateDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var customer = mapper.Map<Models.Entities.Customer>(dto);
            context.Customers.Add(customer);
            await context.SaveChangesAsync(cancellationToken);
            return mapper.Map<CustomerDto>(customer);
        }

        public async Task<CustomerDto> UpdateAsync(int id, CustomerUpdateDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var customer = await context.Customers
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException("Customer not found");
            mapper.Map(dto, customer);
            await context.SaveChangesAsync(cancellationToken);
            return mapper.Map<CustomerDto>(customer);
        }
    }
}
