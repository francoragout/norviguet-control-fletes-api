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
            ArgumentNullException.ThrowIfNull(dto);

            var carrier = mapper.Map<Carrier>(dto);
            context.Carriers.Add(carrier);
            await context.SaveChangesAsync(cancellationToken);
            return mapper.Map<CarrierDto>(carrier);
        }

        public async Task<CarrierDto> UpdateAsync(int id, CarrierUpdateDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

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

            var deliveryNotesTask = context.DeliveryNotes.AnyAsync(dn => dn.CarrierId == id, cancellationToken);
            var invoicesTask = context.Invoices.AnyAsync(i => i.CarrierId == id, cancellationToken);
            var paymentOrdersTask = context.PaymentOrders.AnyAsync(po => po.CarrierId == id, cancellationToken);

            await Task.WhenAll(deliveryNotesTask, invoicesTask, paymentOrdersTask);

            if (await deliveryNotesTask)
                throw new ConflictException("Cannot delete carrier because it is associated with one or more delivery notes");

            if (await invoicesTask)
                throw new ConflictException("Cannot delete carrier because it is associated with one or more invoices");

            if (await paymentOrdersTask)
                throw new ConflictException("Cannot delete carrier because it is associated with one or more payment orders");

            context.Carriers.Remove(carrier);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteBulkAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            // Validación moderna de parámetro
            ArgumentNullException.ThrowIfNull(ids);

            var idList = ids.Distinct().ToList();
            if (idList.Count == 0) return;

            // 1. Verificar existencia de los Carriers
            var existingCarriers = await context.Carriers
                .Where(c => idList.Contains(c.Id))
                .ToListAsync(cancellationToken);

            if (existingCarriers.Count != idList.Count)
                throw new NotFoundException("Some of the specified carriers were not found");

            // 2. Verificación masiva de dependencias (Estrategia de unión)
            // Buscamos todos los IDs que tienen alguna relación en una sola pasada por tabla
            var idsWithConflicts = await context.DeliveryNotes
                .Where(dn => idList.Contains(dn.CarrierId)).Select(dn => dn.CarrierId)
                .Union(context.Invoices
                    .Where(i => idList.Contains(i.CarrierId)).Select(i => i.CarrierId))
                .Union(context.PaymentOrders
                    .Where(po => idList.Contains(po.CarrierId)).Select(po => po.CarrierId))
                .Distinct()
                .ToListAsync(cancellationToken);

            if (idsWithConflicts.Any())
            {
                // Mensaje detallado en inglés
                throw new ConflictException(
                    $"Operation aborted. {idsWithConflicts.Count} carrier(s) cannot be deleted due to existing associations with delivery notes, invoices, or payment orders.");
            }

            // 3. Eliminación en lote
            context.Carriers.RemoveRange(existingCarriers);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
