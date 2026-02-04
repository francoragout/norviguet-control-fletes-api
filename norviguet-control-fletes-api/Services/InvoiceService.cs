using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Common.Middlewares;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.Invoice;
using norviguet_control_fletes_api.Models.Entities;
using norviguet_control_fletes_api.Models.Enums;
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
                ?? throw new NotFoundException("Invoice not found");

            return invoice;
        }

        public async Task<InvoiceDto> CreateAsync(InvoiceCreateDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var order = await context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == dto.OrderId, cancellationToken)
                ?? throw new NotFoundException("Order not found");

            await ValidateCarrierExistsAsync(dto.CarrierId, cancellationToken);

            var exists = await context.Invoices
                .AnyAsync(x => x.Number == dto.Number, cancellationToken);

            if (exists)
            {
                throw new ConflictException($"An invoice with the number '{dto.Number}' already exists");
            }

            if (order.Status != OrderStatus.Pending)
            {
                throw new ConflictException("Invoice can only be created for orders with Pending status");
            }

            await ValidateInvoiceNotExistsForOrderAndCarrierAsync(dto.OrderId, dto.CarrierId, null, cancellationToken);
            await ValidateAllDeliveryNotesApprovedAsync(dto.OrderId, dto.CarrierId, "creating", cancellationToken);

            var invoice = mapper.Map<Invoice>(dto);
            context.Invoices.Add(invoice);
            await context.SaveChangesAsync(cancellationToken);
            return mapper.Map<InvoiceDto>(invoice);
        }


        public async Task<InvoiceDto> UpdateAsync(int id, InvoiceUpdateDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var invoice = await context.Invoices
                .Include(x => x.Order)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new NotFoundException("Invoice not found");

            if (invoice.Order.Status != OrderStatus.Pending)
            {
                throw new ConflictException("Cannot update invoice: the current order is not in Pending status");
            }

            var orderOrCarrierChanged = invoice.OrderId != dto.OrderId || invoice.CarrierId != dto.CarrierId;

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

                await ValidateInvoiceNotExistsForOrderAndCarrierAsync(dto.OrderId, dto.CarrierId, id, cancellationToken);
                await ValidateAllDeliveryNotesApprovedAsync(dto.OrderId, dto.CarrierId, "updating", cancellationToken);
            }

            if (invoice.Number != dto.Number &&
                await context.Invoices.AnyAsync(x => x.Number == dto.Number, cancellationToken))
            {
                throw new ConflictException($"An invoice with the number '{dto.Number}' already exists");
            }

            mapper.Map(dto, invoice);
            context.Entry(invoice).Property(x => x.RowVersion).OriginalValue = dto.RowVersion;

            try
            {
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException(
                    "The record was modified by another user. Please reload and try again.");
            }

            return mapper.Map<InvoiceDto>(invoice);
        }

        public async Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(ids);

            var idList = ids.Distinct().ToList();
            if (idList.Count == 0) return;

            var invoices = await context.Invoices
                .Include(i => i.Order)
                .Where(i => idList.Contains(i.Id))
                .ToListAsync(cancellationToken);

            if (invoices.Count != idList.Count)
                throw new NotFoundException("Some of the specified invoices were not found");

            var notPendingOrders = invoices
                .Where(i => i.Order.Status != OrderStatus.Pending)
                .Select(i => i.Number)
                .ToList();

            if (notPendingOrders.Count > 0)
                throw new ConflictException($"Invoices can only be deleted for orders with Pending status: {string.Join(", ", notPendingOrders)}");

            context.Invoices.RemoveRange(invoices);
            await context.SaveChangesAsync(cancellationToken);
        }

        private async Task ValidateCarrierExistsAsync(int carrierId, CancellationToken cancellationToken)
        {
            var carrierExists = await context.Carriers
                .AnyAsync(x => x.Id == carrierId, cancellationToken);

            if (!carrierExists)
            {
                throw new NotFoundException("Carrier not found");
            }
        }

        private async Task ValidateInvoiceNotExistsForOrderAndCarrierAsync(
            int orderId,
            int carrierId,
            int? excludeInvoiceId,
            CancellationToken cancellationToken)
        {
            var query = context.Invoices
                .Where(x => x.OrderId == orderId && x.CarrierId == carrierId);

            if (excludeInvoiceId.HasValue)
            {
                query = query.Where(x => x.Id != excludeInvoiceId.Value);
            }

            var invoiceExists = await query.AnyAsync(cancellationToken);

            if (invoiceExists)
            {
                throw new ConflictException($"An invoice already exists for Order {orderId} and Carrier {carrierId}");
            }
        }

        private async Task ValidateAllDeliveryNotesApprovedAsync(
            int orderId,
            int carrierId,
            string operationContext,
            CancellationToken cancellationToken)
        {
            var deliveryNotes = await context.DeliveryNotes
                .Where(x => x.OrderId == orderId && x.CarrierId == carrierId)
                .Select(x => new { x.Number, x.Status })
                .ToListAsync(cancellationToken);

            if (deliveryNotes.Count == 0)
            {
                throw new ConflictException("No delivery notes found for this Order and Carrier combination");
            }

            var notApprovedNotes = deliveryNotes
                .Where(x => x.Status != DeliveryNoteStatus.Approved)
                .Select(x => x.Number)
                .ToList();

            if (notApprovedNotes.Count > 0)
            {
                throw new ConflictException($"All delivery notes must be approved before {operationContext} an invoice. Pending delivery notes: {string.Join(", ", notApprovedNotes)}");
            }
        }
    }
}
