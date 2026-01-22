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

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip(dto.GetSkip())
                .Take(dto.PageSize)
                .ProjectTo<OrderDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return new PagedResultDto<OrderDto>
            {
                Items = items,
                TotalItems = totalCount,
                Page = dto.Page,
                PageSize = dto.PageSize
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

            var exists = await context.Orders.AnyAsync(o => o.OrderNumber == dto.OrderNumber, cancellationToken);
            if (exists)
                throw new ConflictException("An order with the same order number already exists");

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

            if (order.Status == OrderStatus.Closed)
                throw new ConflictException("Cannot update a closed order");

            if (order.OrderNumber != dto.OrderNumber)
            {
                var exists = await context.Orders
                    .AnyAsync(o => o.OrderNumber == dto.OrderNumber && o.Id != id, cancellationToken);
                if (exists)
                    throw new ConflictException("An order with the same order number already exists");
            }

            mapper.Map(dto, order);
            await context.SaveChangesAsync(cancellationToken);

            return mapper.Map<OrderDto>(order);
        }

        public async Task<OrderDto> UpdateStatusAsync(int id, OrderStatusUpdateDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var order = await context.Orders
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new NotFoundException("Order not found");

            if (order.Status == OrderStatus.Closed)
                throw new ConflictException("Cannot change status of a closed order");

            order.Status = dto.Status;
            await context.SaveChangesAsync(cancellationToken);

            return mapper.Map<OrderDto>(order);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var rowsDeleted = await context.Orders
                .Where(x => x.Id == id && x.Status != OrderStatus.Closed)
                .ExecuteDeleteAsync(cancellationToken);

            if (rowsDeleted == 0)
            {
                var exists = await context.Orders.AnyAsync(x => x.Id == id, cancellationToken);
                if (!exists)
                    throw new NotFoundException("Order not found");
                else
                    throw new ConflictException("Cannot delete a closed order");
            }
        }

        public async Task DeleteBulkAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(ids);

            var idsList = ids.ToList();
            var hasClosedOrders = await context.Orders
                .Where(x => idsList.Contains(x.Id) && x.Status == OrderStatus.Closed)
                .AnyAsync(cancellationToken);

            if (hasClosedOrders)
                throw new ConflictException("Cannot delete closed orders");

            await context.Orders
                .Where(x => idsList.Contains(x.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
