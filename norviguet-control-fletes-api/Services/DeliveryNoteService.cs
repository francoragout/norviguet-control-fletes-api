using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Common.Middlewares;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.Common;
using norviguet_control_fletes_api.Models.DTOs.DeliveryNote;
using norviguet_control_fletes_api.Models.Entities;
using norviguet_control_fletes_api.Services.Interfaces;

namespace norviguet_control_fletes_api.Services
{
    public class DeliveryNoteService(ApplicationDbContext context, IMapper mapper) : IDeliveryNoteService
    {
        public async Task<PagedResultDto<DeliveryNoteDto>> GetAllAsync(PagedRequestDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var query = context.DeliveryNotes
                .AsNoTracking()
                .OrderByDescending(dn => dn.CreatedAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip(dto.GetSkip())
                .Take(dto.PageSize)
                .ProjectTo<DeliveryNoteDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return new PagedResultDto<DeliveryNoteDto>
            {
                Items = items,
                Page = dto.Page,
                PageSize = dto.PageSize,
                TotalItems = totalCount
            };
        }

        public async Task<DeliveryNoteDto> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var deliveryNote = await context.DeliveryNotes
                .AsNoTracking()
                .Where(x => x.Id == id)
                .ProjectTo<DeliveryNoteDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException("Delivery note not found");
            return deliveryNote;
        }

        public async Task<DeliveryNoteDto> CreateAsync(DeliveryNoteCreateDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var deliveryNote = mapper.Map<DeliveryNote>(dto);
            context.DeliveryNotes.Add(deliveryNote);
            await context.SaveChangesAsync(cancellationToken);
            return mapper.Map<DeliveryNoteDto>(deliveryNote);
        }

        public async Task<DeliveryNoteDto> UpdateAsync(int id, DeliveryNoteUpdateDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var deliveryNote = await context.DeliveryNotes
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new NotFoundException("Delivery note not found");
            mapper.Map(dto, deliveryNote);
            await context.SaveChangesAsync(cancellationToken);
            return mapper.Map<DeliveryNoteDto>(deliveryNote);
        }

        public async Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(ids);
            var idList = ids.Distinct().ToList();
            if (idList.Count == 0) return;

            var existingIds = await context.DeliveryNotes
                .Where(dn => idList.Contains(dn.Id))
                .Select(dn => dn.Id)
                .ToListAsync(cancellationToken);

            if (existingIds.Count != idList.Count)
                throw new NotFoundException("Some of the specified delivery notes were not found");

            await context.DeliveryNotes
                .Where(dn => idList.Contains(dn.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
