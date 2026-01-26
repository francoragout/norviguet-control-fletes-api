using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Common.Middlewares;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.Common;
using norviguet_control_fletes_api.Models.DTOs.PaymentOrder;
using norviguet_control_fletes_api.Models.Entities;
using norviguet_control_fletes_api.Services.Interfaces;

namespace norviguet_control_fletes_api.Services
{
    public class PaymentOrderService(ApplicationDbContext context, IMapper mapper) : IPaymentOrderService
    {
        public async Task<PagedResultDto<PaymentOrderDto>> GetAllAsync(PagedRequestDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var query = context.PaymentOrders
                .AsNoTracking()
                .OrderByDescending(po => po.CreatedAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip(dto.GetSkip())
                .Take(dto.PageSize)
                .ProjectTo<PaymentOrderDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return new PagedResultDto<PaymentOrderDto>
            {
                Items = items,
                Page = dto.Page,
                PageSize = dto.PageSize,
                TotalItems = totalCount
            };
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
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new NotFoundException("Payment order not found");
            mapper.Map(dto, paymentOrder);
            await context.SaveChangesAsync(cancellationToken);
            return mapper.Map<PaymentOrderDto>(paymentOrder);
        }

        public async Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(ids);
            var idList = ids.Distinct().ToList();
            if (idList.Count == 0) return;

            var existingIds = await context.PaymentOrders
                .Where(po => idList.Contains(po.Id))
                .Select(po => po.Id)
                .ToListAsync(cancellationToken);

            if (existingIds.Count != idList.Count)
                throw new NotFoundException("Some of the specified payment orders were not found");

            await context.PaymentOrders
                .Where(po => idList.Contains(po.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
