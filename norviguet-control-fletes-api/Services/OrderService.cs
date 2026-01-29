using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Common.Middlewares;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.Common;
using norviguet_control_fletes_api.Models.DTOs.Order;
using norviguet_control_fletes_api.Models.Entities;
using norviguet_control_fletes_api.Models.Enums;
using norviguet_control_fletes_api.Services.Interfaces;

namespace norviguet_control_fletes_api.Services
{
    public class OrderService(ApplicationDbContext context, IMapper mapper) : IOrderService
    {
        public async Task<PagedResultDto<OrderDto>> GetAllAsync(PagedRequestDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var query = context.Orders
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedAt);

            var totalItems = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip(dto.GetSkip())
                .Take(dto.PageSize)
                .ProjectTo<OrderDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return new PagedResultDto<OrderDto>
            {
                Items = items,
                Page = dto.Page,
                PageSize = dto.PageSize,
                TotalItems = totalItems
            };

        }

        public async Task<OrderDto> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var order = await context.Orders
                .AsNoTracking()
                .Where(x => x.Id == id)
                .ProjectTo<OrderDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException("Order not found");
            return order;
        }

        public async Task<OrderDto> CreateAsync(OrderCreateDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var order = mapper.Map<Order>(dto);
            context.Orders.Add(order);
            await context.SaveChangesAsync(cancellationToken);
            return mapper.Map<OrderDto>(order);
        }

        public async Task<OrderDto> UpdateAsync(int id, OrderUpdateDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var order = await context.Orders
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new NotFoundException("Order not found");

            mapper.Map(dto, order);

            try
            {
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException("The record you attempted to edit was modified by another user after you got the original value.");
            }

            return mapper.Map<OrderDto>(order);
        }

        public async Task<OrderDto> UpdateStatusAsync(int id, OrderStatusUpdateDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var order = await context.Orders
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new NotFoundException("Order not found");

            if (!Enum.TryParse<OrderStatus>(dto.Status, true, out var status) ||
                !Enum.IsDefined(typeof(OrderStatus), status))
            {
                throw new ArgumentException($"Invalid status: {dto.Status}");
            }

            order.Status = status;
            await context.SaveChangesAsync(cancellationToken);
            return mapper.Map<OrderDto>(order);
        }

        public async Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(ids); 

            var idList = ids.Distinct().ToList();
            if (idList.Count == 0) return;

            var existingCount = await context.Orders
                .CountAsync(o => idList.Contains(o.Id), cancellationToken);

            if (existingCount != idList.Count)
                throw new NotFoundException("Some orders were not found.");

            await context.Orders
                .Where(o => idList.Contains(o.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
