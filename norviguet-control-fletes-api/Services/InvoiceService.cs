using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.Invoice;
using norviguet_control_fletes_api.Models.Entities;
using norviguet_control_fletes_api.Services.Interfaces;

namespace norviguet_control_fletes_api.Services
{
    public class InvoiceService(ApplicationDbContext context, IMapper mapper) : IInvoiceService
    {
        public async Task<IReadOnlyList<InvoiceDto>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await context.Invoices
                .AsNoTracking()
                .OrderByDescending(i => i.CreatedAt)
                .ProjectTo<InvoiceDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        public async Task<InvoiceDto> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var invoice = await context.Invoices
                .AsNoTracking()
                .Where(x => x.Id == id)
                .ProjectTo<InvoiceDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException("Invoice not found");

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
                ?? throw new KeyNotFoundException("Invoice not found");

            mapper.Map(dto, invoice);

            try
            {
                await context.SaveChangesAsync(cancellationToken);

            }
            catch (DbUpdateConcurrencyException)
            {
                throw new InvalidOperationException("The record you attempted to edit was modified by another user after you got the original value.");
            }

            return mapper.Map<InvoiceDto>(invoice);
        }

        public async Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(ids);

            var idList = ids.Distinct().ToList();
            if (idList.Count == 0) return;

            var invoices = await context.Invoices
                .Where(i => idList.Contains(i.Id))
                .Select(i => i.Id)
                .ToListAsync(cancellationToken);

            if (invoices.Count != idList.Count)
                throw new KeyNotFoundException("Some of the specified invoices were not found");

            await context.Invoices
                .Where(i => idList.Contains(i.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
