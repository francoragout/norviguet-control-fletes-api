using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Common.Middlewares;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.DeliveryNote;
using norviguet_control_fletes_api.Models.Entities;
using norviguet_control_fletes_api.Models.Enums;
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

            // 1. Check for unique DeliveryNoteNumber
            var deliveryNoteNumberExists = await context.DeliveryNotes
                .AnyAsync(d => d.DeliveryNoteNumber == dto.DeliveryNoteNumber, cancellationToken);

            if (deliveryNoteNumberExists)
                throw new ConflictException(
                    $"A delivery note with the number '{dto.DeliveryNoteNumber}' already exists");

            // 2. Check that the associated Order is not Closed or Rejected
            var orderStatus = await context.Orders
                .Where(o => o.Id == dto.OrderId)
                .Select(o => (OrderStatus?)o.Status)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException("Order not found");

            if (orderStatus is OrderStatus.Closed or OrderStatus.Rejected)
                throw new ConflictException(
                    $"Cannot create a delivery note for an order with status {orderStatus}.");

            var deliveryNote = mapper.Map<DeliveryNote>(dto);
            context.DeliveryNotes.Add(deliveryNote);
            await context.SaveChangesAsync(cancellationToken);
            return mapper.Map<DeliveryNoteDto>(deliveryNote);
        }

        public async Task<DeliveryNoteDto> UpdateAsync(int id, DeliveryNoteUpdateDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            // 1. Retrieve existing delivery note
            var deliveryNote = await context.DeliveryNotes
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken)
                ?? throw new NotFoundException("Delivery note not found");

            // 2. Check that the delivery note is not Approved or Cancelled
            if (deliveryNote.Status is DeliveryNoteStatus.Approved or DeliveryNoteStatus.Cancelled)
                throw new ConflictException(
                    $"Cannot update a delivery note with status '{deliveryNote.Status}'");

            // 3. Check for unique DeliveryNoteNumber
            if (deliveryNote.DeliveryNoteNumber != dto.DeliveryNoteNumber)
            {
                var deliverNoteyNumberExists = await context.DeliveryNotes
                    .AnyAsync(d => d.DeliveryNoteNumber == dto.DeliveryNoteNumber, cancellationToken);

                if (deliverNoteyNumberExists)
                    throw new ConflictException(
                        $"A delivery note with the number '{dto.DeliveryNoteNumber}' already exists");
            }

            // 4. Check that the associated Order is not Closed or Rejected
            var orderStatus = await context.Orders
                .Where(o => o.Id == dto.OrderId)
                .Select(o => (OrderStatus?)o.Status)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException("Order not found");

            if (orderStatus is OrderStatus.Closed or OrderStatus.Rejected)
                throw new ConflictException(
                    $"Cannot update a delivery note for an order with status {orderStatus}.");

            // 5. Map and save changes with concurrency handling
            mapper.Map(dto, deliveryNote);
            context.Entry(deliveryNote).Property(d => d.RowVersion).OriginalValue = dto.RowVersion;

            try
            {
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException(
                    "The record was modified by another user. Please reload and try again.");
            }

            return mapper.Map<DeliveryNoteDto>(deliveryNote);
        }

        public async Task<DeliveryNoteDto> UpdateStatusAsync(int id, DeliveryNoteStatusUpdateDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            // 1. Retrieve existing delivery note
            var deliveryNote = await context.DeliveryNotes
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken)
                ?? throw new NotFoundException("Delivery note not found");

            // 2. Validate and parse new status
            if (!Enum.TryParse<DeliveryNoteStatus>(dto.Status, true, out var newStatus) ||
                !Enum.IsDefined(typeof(DeliveryNoteStatus), newStatus))
            {
                throw new ArgumentException($"Invalid status: {dto.Status}");
            }

            deliveryNote.Status = newStatus;
            await context.SaveChangesAsync(cancellationToken);
            return mapper.Map<DeliveryNoteDto>(deliveryNote);
        }

        public async Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(ids);

            var idList = ids.Distinct().ToList();
            if (idList.Count == 0) return;

            // 1. Validate existence and business rules in one efficient query
            var validatableNotes = await context.DeliveryNotes
                .Where(d => idList.Contains(d.Id))
                .Select(d => new { d.Id, IsClosed = d.Order.Status == OrderStatus.Closed })
                .ToListAsync(cancellationToken);

            // 2. Check if everything was found
            if (validatableNotes.Count != idList.Count)
                throw new NotFoundException("Some delivery notes were not found.");

            // 3. Check business rules
            if (validatableNotes.Any(n => n.IsClosed))
                throw new ConflictException("Cannot delete delivery notes associated with closed orders.");

            // 4. Perform the bulk delete
            await context.DeliveryNotes
                .Where(d => idList.Contains(d.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }

        private async Task EnsureNumberIsUniqueAsync(string deliveryNoteNumber, int? excludeId, CancellationToken cancellationToken)
        {
            var exists = await context.DeliveryNotes
                .AnyAsync(d => d.DeliveryNoteNumber == deliveryNoteNumber && (!excludeId.HasValue || d.Id != excludeId.Value), cancellationToken);
            if (exists)
                throw new ConflictException(
                    $"A delivery note with the number '{deliveryNoteNumber}' already exists");
        }
    }
}
