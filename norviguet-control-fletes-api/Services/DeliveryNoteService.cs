using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Common.Middlewares;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.DeliveryNote;
using norviguet_control_fletes_api.Models.Entities;
using norviguet_control_fletes_api.Services.Interfaces;

namespace norviguet_control_fletes_api.Services
{
    public class DeliveryNoteService(ApplicationDbContext context, IMapper mapper) : IDeliveryNoteService
    {
        public async Task<IReadOnlyList<DeliveryNoteDto>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await context.DeliveryNotes
                .AsNoTracking()
                .OrderByDescending(d => d.CreatedAt)
                .ProjectTo<DeliveryNoteDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
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
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken)
                ?? throw new NotFoundException("Delivery note not found");

            mapper.Map(dto, deliveryNote);

            try
            {
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException("The record you attempted to edit was modified by another user after you got the original value.");
            }

            return mapper.Map<DeliveryNoteDto>(deliveryNote);
        }

        public async Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(ids);

            var idList = ids.Distinct().ToList();
            if (idList.Count == 0) return;

            var deliveryNotes = await context.DeliveryNotes
                .Where(d => idList.Contains(d.Id))
                .Select(d => d.Id)
                .ToListAsync(cancellationToken);

            if (deliveryNotes.Count != idList.Count)
                throw new NotFoundException("Some of the specified delivery notes were not found");

            await context.DeliveryNotes
                .Where(d => idList.Contains(d.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
