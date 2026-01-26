using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Common.Middlewares;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.Common;
using norviguet_control_fletes_api.Models.DTOs.Invoice;
using norviguet_control_fletes_api.Models.Entities;
using norviguet_control_fletes_api.Services.Interfaces;

namespace norviguet_control_fletes_api.Services
{
    public class InvoiceService(ApplicationDbContext context, IMapper mapper) : IInvoiceService
    {
        public async Task<PagedResultDto<InvoiceDto>> GetAllAsync(PagedRequestDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var query = context.Invoices
                .AsNoTracking()
                .OrderByDescending(i => i.CreatedAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip(dto.GetSkip())
                .Take(dto.PageSize)
                .ProjectTo<InvoiceDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return new PagedResultDto<InvoiceDto>
            {
                Items = items,
                Page = dto.Page,
                PageSize = dto.PageSize,
                TotalItems = totalCount
            };
        }

        public async Task<InvoiceDto> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var invoice = await context.Invoices
                .AsNoTracking()
                .Where(x => x.Id == id)
                .ProjectTo<InvoiceDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException("Invoice not found");
            return invoice;
        }

        public async Task<InvoiceDto> CreateAsync(InvoiceCreateDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var invoice = mapper.Map<Invoice>(dto);
            context.Invoices.Add(invoice);
            await context.SaveChangesAsync(cancellationToken);
            return mapper.Map<InvoiceDto>(invoice);
        }

        public async Task<InvoiceDto> UpdateAsync(int id, InvoiceUpdateDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var invoice = await context.Invoices
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new NotFoundException("Invoice not found");
            mapper.Map(dto, invoice);
            await context.SaveChangesAsync(cancellationToken);
            return mapper.Map<InvoiceDto>(invoice);
        }

        public async Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(ids);
            var idList = ids.Distinct().ToList();
            if (idList.Count == 0) return;

            var existingIds = await context.Invoices
                .Where(i => idList.Contains(i.Id))
                .Select(i => i.Id)
                .ToListAsync(cancellationToken);

            if (existingIds.Count != idList.Count)
                throw new NotFoundException("Some of the specified invoices were not found");

            await context.Invoices
                .Where(i => idList.Contains(i.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
