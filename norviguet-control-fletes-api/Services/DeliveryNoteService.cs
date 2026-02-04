using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Common.Middlewares;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.DeliveryNote;
using norviguet_control_fletes_api.Models.Entities;
using norviguet_control_fletes_api.Models.Enums;
using norviguet_control_fletes_api.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

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

            var order = await context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == dto.OrderId, cancellationToken)
                ?? throw new NotFoundException("Order not found");

            await ValidateCarrierExistsAsync(dto.CarrierId, cancellationToken);

            var exists = await context.DeliveryNotes
                .AnyAsync(x => x.Number == dto.Number, cancellationToken);

            if (exists)
            {
                throw new ConflictException($"A delivery note with the number '{dto.Number}' already exists");
            }

            if (order.Status != OrderStatus.Pending)
            {
                throw new ConflictException("Delivery notes can only be created for orders with Pending status");
            }

            var deliveryNote = mapper.Map<DeliveryNote>(dto);
            context.DeliveryNotes.Add(deliveryNote);
            await context.SaveChangesAsync(cancellationToken);
            return mapper.Map<DeliveryNoteDto>(deliveryNote);
        }

        public async Task<DeliveryNoteDto> UpdateAsync(int id, DeliveryNoteUpdateDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var deliveryNote = await context.DeliveryNotes
                .Include(x => x.Order)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new NotFoundException("Delivery note not found");

            if (deliveryNote.Order.Status != OrderStatus.Pending)
            {
                throw new ConflictException("Delivery notes can only be updated for orders with Pending status");
            }

            var orderOrCarrierChanged = deliveryNote.OrderId != dto.OrderId ||
                                        deliveryNote.CarrierId != dto.CarrierId;

            if (orderOrCarrierChanged)
            {
                var order = await context.Orders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == dto.OrderId, cancellationToken)
                    ?? throw new NotFoundException("Order not found");

                await ValidateCarrierExistsAsync(dto.CarrierId, cancellationToken);

                if (order.Status != OrderStatus.Pending)
                {
                    throw new ConflictException("Cannot change to this order: the target order is not in Pending status");
                }
            }

            if (deliveryNote.Number != dto.Number &&
                 await context.DeliveryNotes.AnyAsync(x => x.Number == dto.Number, cancellationToken))
            {
                throw new ConflictException($"A delivery note with the number '{dto.Number}' already exists");
            }

            mapper.Map(dto, deliveryNote);
            context.Entry(deliveryNote).Property(x => x.RowVersion).OriginalValue = dto.RowVersion;

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

            var deliveryNote = await context.DeliveryNotes
                .Include(x => x.Order)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new NotFoundException("Delivery note not found");

            if (deliveryNote.Order.Status != OrderStatus.Pending)
            {
                throw new ConflictException("Delivery note status can only be updated for orders with Pending status");
            }

            if (!Enum.TryParse<DeliveryNoteStatus>(dto.Status, true, out var status))
            {
                throw new ValidationException($"Invalid status value: '{dto.Status}'");
            }

            deliveryNote.Status = status;
            context.Entry(deliveryNote).Property(x => x.RowVersion).OriginalValue = dto.RowVersion;

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

        public async Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(ids);

            var idList = ids.Distinct().ToList();

            if (idList.Count == 0) return;

            var deliveryNotes = await context.DeliveryNotes
                .AsNoTracking()
                .Include(d => d.Order)
                .Where(d => idList.Contains(d.Id))
                .ToListAsync(cancellationToken);

            if (deliveryNotes.Count != idList.Count)
                throw new NotFoundException("Some of the specified delivery notes were not found");

            var invalidDeliveryNotes = deliveryNotes
                .Where(d => d.Status != DeliveryNoteStatus.Pending && d.Status != DeliveryNoteStatus.Cancelled)
                .Select(d => d.Number)
                .ToList();

            if (invalidDeliveryNotes.Count > 0)
                throw new ConflictException($"Delivery notes can only be deleted if they are Pending or Cancelled. Invalid delivery notes: {string.Join(", ", invalidDeliveryNotes)}");

            var invalidOrders = deliveryNotes
                .Where(d => d.Order.Status != OrderStatus.Pending && d.Order.Status != OrderStatus.Rejected)
                .Select(d => d.Number)
                .ToList();

            if (invalidOrders.Count > 0)
                throw new ConflictException($"Delivery notes can only be deleted for orders with Pending or Rejected status. Invalid delivery notes: {string.Join(", ", invalidOrders)}");

            await context.DeliveryNotes
                .Where(d => idList.Contains(d.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }

        // Private validation methods
        private async Task ValidateCarrierExistsAsync(int carrierId, CancellationToken cancellationToken)
        {
            var carrierExists = await context.Carriers
                .AnyAsync(x => x.Id == carrierId, cancellationToken);

            if (!carrierExists)
            {
                throw new NotFoundException("Carrier not found");
            }
        }
    }
}
