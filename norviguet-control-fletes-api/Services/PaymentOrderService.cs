using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Common.Middlewares;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.PaymentOrder;
using norviguet_control_fletes_api.Models.Entities;
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

            var paymentOrder = mapper.Map<PaymentOrder>(dto);
            context.PaymentOrders.Add(paymentOrder);
            await context.SaveChangesAsync(cancellationToken);
            return mapper.Map<PaymentOrderDto>(paymentOrder);
        }

        public async Task<PaymentOrderDto> UpdateAsync(int id, PaymentOrderUpdateDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var paymentOrder = await context.PaymentOrders
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
                ?? throw new NotFoundException("Payment order not found");

            mapper.Map(dto, paymentOrder);

            try
            {
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException("The record you attempted to edit was modified by another user after you got the original value.");
            }

            return mapper.Map<PaymentOrderDto>(paymentOrder);
        }

        public async Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(ids);

            var idList = ids.Distinct().ToList();
            if (idList.Count == 0) return;

            var paymentOrders = await context.PaymentOrders
                .Where(p => idList.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            if (paymentOrders.Count != idList.Count)
                throw new NotFoundException("Some of the specified payment orders were not found");

            await context.PaymentOrders
                .Where(p => idList.Contains(p.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
