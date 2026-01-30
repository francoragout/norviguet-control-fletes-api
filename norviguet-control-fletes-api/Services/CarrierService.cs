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

            var totalItems = await query.CountAsync(cancellationToken);

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
                TotalItems = totalItems
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

            var nameExists = await context.Carriers
                .AnyAsync(c => c.Name == dto.Name, cancellationToken);

            if (nameExists)
                throw new ConflictException(
                    $"A carrier with the name '{dto.Name}' already exists");

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

            if (carrier.Name != dto.Name)
            {
                var nameExists = await context.Carriers
                    .AnyAsync(c => c.Name == dto.Name && c.Id != id, cancellationToken);

                if (nameExists)
                    throw new ConflictException(
                        $"A carrier with the name '{dto.Name}' already exists");
            }

            mapper.Map(dto, carrier);
            context.Entry(carrier).Property(x => x.RowVersion).OriginalValue = dto.RowVersion;

            try
            {
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException(
                    "The record was modified by another user. Please reload and try again.");
            }

            return mapper.Map<CarrierDto>(carrier);
        }


        public async Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(ids);

            var idList = ids.Distinct().ToList();

            if (idList.Count == 0) return;

            var existingIds = await context.Carriers
                .Where(c => idList.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);

            if (existingIds.Count != idList.Count)
                throw new NotFoundException("Some of the specified carriers were not found");

            var conflictIds = await context.DeliveryNotes
                .Where(dn => idList.Contains(dn.CarrierId)).Select(dn => dn.CarrierId)
                .Union(context.Invoices.Where(i => idList.Contains(i.CarrierId)).Select(i => i.CarrierId))
                .Union(context.PaymentOrders.Where(po => idList.Contains(po.CarrierId)).Select(po => po.CarrierId))
                .Distinct()
                .ToListAsync(cancellationToken);

            if (conflictIds.Any())
            {
                throw new ConflictException(
                    $"Operation aborted. {conflictIds.Count} carrier(s) cannot be deleted due to existing associations with delivery notes, invoices, or payment orders.");
            }

            await context.Carriers
                .Where(c => idList.Contains(c.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
