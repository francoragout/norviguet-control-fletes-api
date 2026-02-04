using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Common.Middlewares;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.PaymentOrder;
using norviguet_control_fletes_api.Models.Entities;
using norviguet_control_fletes_api.Models.Enums;
using norviguet_control_fletes_api.Services.Interfaces;

namespace norviguet_control_fletes_api.Services
{
    public class PaymentOrderService(ApplicationDbContext context, IMapper mapper) : IPaymentOrderService
    {
        public async Task<IReadOnlyList<PaymentOrderDto>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await context.PaymentOrders
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ProjectTo<PaymentOrderDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        public async Task<PaymentOrderDto> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var paymentOrder = await context.PaymentOrders
                .AsNoTracking()
                .Where(x => x.Id == id)
                .ProjectTo<PaymentOrderDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException("Payment order not found");
            return paymentOrder;
        }

        public async Task<PaymentOrderDto> CreateAsync(PaymentOrderCreateDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var order = await context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == dto.OrderId, cancellationToken)
                ?? throw new NotFoundException("Order not found");

            await ValidateCarrierExistsAsync(dto.CarrierId, cancellationToken);

            var exists = await context.PaymentOrders
                .AnyAsync(x => x.Number == dto.Number, cancellationToken);

            if (exists)
            {
                throw new ConflictException($"A payment order with the same number '{dto.Number}' already exists");
            }

            if (order.Status != OrderStatus.Pending)
            {
                throw new ConflictException("Invoice can only be created for orders with Pending status");
            }

            await ValidatePaymentOrderNotExistsForOrderAndCarrierAsync(dto.OrderId, dto.CarrierId, null, cancellationToken);
            await ValidateInvoiceNotExistsForOrderAndCarrierAsync(dto.OrderId, dto.CarrierId, null, cancellationToken);

            var paymentOrder = mapper.Map<PaymentOrder>(dto);
            context.PaymentOrders.Add(paymentOrder);
            await context.SaveChangesAsync(cancellationToken);
            return mapper.Map<PaymentOrderDto>(paymentOrder);
        }

        public async Task<PaymentOrderDto> UpdateAsync(int id, PaymentOrderUpdateDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var paymentOrder = await context.PaymentOrders
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new NotFoundException("Payment order not found");

            if (paymentOrder.Order.Status != OrderStatus.Pending)
            {
                throw new ConflictException("Cannot update payment order: the current order is not in Pending status");
            }

            var orderOrCarrierChanged = paymentOrder.OrderId != dto.OrderId ||
                                        paymentOrder.CarrierId != dto.CarrierId;

            if (orderOrCarrierChanged)
            {
                var order = await context.Orders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == dto.OrderId, cancellationToken)
                    ?? throw new NotFoundException("Order not found");

                await ValidateCarrierExistsAsync(dto.CarrierId, cancellationToken);

                if (order.Status != OrderStatus.Pending)
                {
                    throw new ConflictException("Cannot update payment order: the new order is not in Pending status");
                }

                await ValidatePaymentOrderNotExistsForOrderAndCarrierAsync(dto.OrderId, dto.CarrierId, paymentOrder.Id, cancellationToken);
                await ValidateInvoiceNotExistsForOrderAndCarrierAsync(dto.OrderId, dto.CarrierId, paymentOrder.Id, cancellationToken);
            }

            if (paymentOrder.Number != dto.Number)
            {
                var numberExists = await context.PaymentOrders
                    .AnyAsync(x => x.Number == dto.Number && x.Id != id, cancellationToken);
                if (numberExists)
                {
                    throw new ConflictException($"A payment order with the same number '{dto.Number}' already exists");
                }
            }

            mapper.Map(dto, paymentOrder);
            context.Entry(paymentOrder).Property(x => x.RowVersion).OriginalValue = dto.RowVersion;

            try
            {
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException(
                    "The record was modified by another user. Please reload and try again.");
            }

            return mapper.Map<PaymentOrderDto>(paymentOrder);
        }

        public async Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(ids);

            var idList = ids.Distinct().ToList();

            if (idList.Count == 0) return;

            var paymentOrders = await context.PaymentOrders
                .AsNoTracking()
                .Include(po => po.Order)
                .Where(po => idList.Contains(po.Id))
                .ToListAsync(cancellationToken);

            if (paymentOrders.Count != idList.Count)
                throw new NotFoundException("Some of the specified payment order were not found");

            var invalidOrders = paymentOrders
               .Where(po => po.Order.Status != OrderStatus.Pending && po.Order.Status != OrderStatus.Rejected)
               .Select(po => po.Number)
               .ToList();

            if (invalidOrders.Count > 0)
                throw new ConflictException($"Payment orders can only be deleted for orders with Pending or Rejected status. Invalid payment orders: {string.Join(", ", invalidOrders)}");

            await context.PaymentOrders
                .Where(po => idList.Contains(po.Id))
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

        private async Task ValidatePaymentOrderNotExistsForOrderAndCarrierAsync(
            int orderId,
            int carrierId,
            int? excludeInvoiceId,
            CancellationToken cancellationToken)
        {
            var query = context.PaymentOrders
                .Where(x => x.OrderId == orderId && x.CarrierId == carrierId);

            if (excludeInvoiceId.HasValue)
            {
                query = query.Where(x => x.Id != excludeInvoiceId.Value);
            }

            var paymentOrderExists = await query.AnyAsync(cancellationToken);

            if (paymentOrderExists)
            {
                throw new ConflictException($"A payment order already exists for Order {orderId} and Carrier {carrierId}");
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
    }
}
