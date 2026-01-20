using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Common.Middlewares;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.Carrier;
using norviguet_control_fletes_api.Models.DTOs.Common;
using norviguet_control_fletes_api.Models.Entities;
using norviguet_control_fletes_api.Services.Interfaces;

namespace norviguet_control_fletes_api.Services
{
    public class CarrierService(ApplicationDbContext context, IMapper mapper) : ICarrierService
    {
        /// <summary>
        /// Retrives a paginated list of all carriers.
        /// </summary>
        public async Task<PagedResultDto<CarrierDto>> GetAllAsync(PagedRequestDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var query = context.Carriers
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip(dto.GetSkip())
                .Take(dto.PageSize)
                .ProjectTo<CarrierDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return new PagedResultDto<CarrierDto>
            {
                Items = items,
                Page = dto.Page,
                PageSize = dto.PageSize,
                TotalItems = totalCount
            };
        }

        /// <summary>
        /// Retrives a carrier by its ID.
        /// </summary>

        public async Task<CarrierDto> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var carrier = await context.Carriers
                .AsNoTracking()
                .Where(x => x.Id == id)
                .ProjectTo<CarrierDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException("Carrier not found");
            return carrier;
        }

        public async Task<CarrierDto> CreateAsync(CarrierCreateDto dto, CancellationToken cancellationToken)
        {
            var carrier = mapper.Map<Carrier>(dto);
            context.Carriers.Add(carrier);
            await context.SaveChangesAsync(cancellationToken);
            return mapper.Map<CarrierDto>(carrier);
        }

        public async Task<CarrierDto> UpdateAsync(int id, CarrierUpdateDto dto, CancellationToken cancellationToken)
        {
            var carrier = await context.Carriers
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new NotFoundException("Carrier not found");
            mapper.Map(dto, carrier);
            await context.SaveChangesAsync(cancellationToken);
            return mapper.Map<CarrierDto>(carrier);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var carrier = await context.Carriers
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
               ?? throw new NotFoundException("Carrier not found");
            var hasDeliveryNotes = await context.DeliveryNotes
                .AnyAsync(dn => dn.CarrierId == id, cancellationToken);
            if (hasDeliveryNotes)
                throw new ConflictException("Cannot delete carrier because it is associated with one or more delivery notes");
            var hasInvoices = await context.Invoices
                .AnyAsync(i => i.CarrierId == id, cancellationToken);
            if (hasInvoices)
                throw new ConflictException("Cannot delete carrier because it is associated with one or more invoices");
            var hasPaymentOrders = await context.PaymentOrders
                .AnyAsync(po => po.CarrierId == id, cancellationToken);
            if (hasPaymentOrders)
                throw new ConflictException("Cannot delete carrier because it is associated with one or more payment orders");
            context.Carriers.Remove(carrier);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteBulkAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            var carrierIds = ids.ToList();
            var hasDeliveryNotes = await context.DeliveryNotes
                .AnyAsync(dn => carrierIds.Contains(dn.CarrierId), cancellationToken);
            if (hasDeliveryNotes)
                throw new ConflictException("Cannot delete one or more carriers because they are associated with delivery notes");
            var hasInvoices = await context.Invoices
                .AnyAsync(i => carrierIds.Contains(i.CarrierId), cancellationToken);
            if (hasInvoices)
                throw new ConflictException("Cannot delete one or more carriers because they are associated with invoices");
            var hasPaymentOrders = await context.PaymentOrders
                .AnyAsync(po => carrierIds.Contains(po.CarrierId), cancellationToken);
            if (hasPaymentOrders)
                throw new ConflictException("Cannot delete one or more carriers because they are associated with payment orders");
            await context.Carriers
                .Where(x => carrierIds.Contains(x.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
