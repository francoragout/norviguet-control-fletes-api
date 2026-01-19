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
        /// <summary>
        /// Retrieves a paginated list of orders.
        /// </summary>
        public async Task<PagedResultDto<OrderDto>> GetAllAsync(PagedRequestDto request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var query = context.Orders
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip(request.GetSkip())
                .Take(request.PageSize)
                .ProjectTo<OrderDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return new PagedResultDto<OrderDto>
            {
                Items = items,
                TotalItems = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        /// <summary>
        /// Retrieves an order by its ID.
        /// </summary>
        public async Task<OrderDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var order = await context.Orders
                .AsNoTracking()
                .Where(x => x.Id == id)
                .ProjectTo<OrderDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException("Order not found");
            return order;
        }

        /// <summary>
        /// Creates a new order.
        /// </summary>
        public async Task<OrderDto> CreateAsync(OrderCreateDto dto, CancellationToken cancellationToken = default)
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

        /// <summary>
        /// Updates an existing order.
        /// </summary>
        public async Task<OrderDto> UpdateAsync(int id, OrderUpdateDto dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var order = await context.Orders
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new KeyNotFoundException("Order not found");

            if (order.Status == OrderStatus.Closed)
                throw new InvalidOperationException("Cannot update a closed order");

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

        /// <summary>
        /// Updates the status of an order.
        /// </summary>
        public async Task<OrderDto> UpdateStatusAsync(int id, OrderStatusUpdateDto dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var order = await context.Orders
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new KeyNotFoundException("Order not found");

            if (order.Status == OrderStatus.Closed)
                throw new InvalidOperationException("Cannot change status of a closed order");

            order.Status = dto.Status;
            await context.SaveChangesAsync(cancellationToken);

            return mapper.Map<OrderDto>(order);
        }

        /// <summary>
        /// Deletes an order by its ID.
        /// </summary>
        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var rowsDeleted = await context.Orders
                .Where(x => x.Id == id && x.Status != OrderStatus.Closed)
                .ExecuteDeleteAsync(cancellationToken);

            if (rowsDeleted == 0)
            {
                var exists = await context.Orders.AnyAsync(x => x.Id == id, cancellationToken);
                if (!exists)
                    throw new KeyNotFoundException("Order not found");
                else
                    throw new InvalidOperationException("Cannot delete a closed order");
            }
        }

        /// <summary>
        /// Deletes multiple orders by their IDs.
        /// </summary>
        public async Task DeleteBulkAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(ids);

            var idsList = ids.ToList();
            var hasClosedOrders = await context.Orders
                .Where(x => idsList.Contains(x.Id) && x.Status == OrderStatus.Closed)
                .AnyAsync(cancellationToken);

            if (hasClosedOrders)
                throw new InvalidOperationException("Cannot delete closed orders");

            await context.Orders
                .Where(x => idsList.Contains(x.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
