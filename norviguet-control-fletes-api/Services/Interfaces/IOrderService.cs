using norviguet_control_fletes_api.Models.DTOs.Common;
using norviguet_control_fletes_api.Models.DTOs.Order;

namespace norviguet_control_fletes_api.Services.Interfaces
{
    public interface IOrderService
    {
        Task<PagedResultDto<OrderDto>> GetAllAsync(PagedRequestDto request, CancellationToken cancellationToken = default);
        Task<OrderDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<OrderDto> CreateAsync(OrderCreateDto dto, CancellationToken cancellationToken = default);
        Task<OrderDto> UpdateAsync(int id, OrderUpdateDto dto, CancellationToken cancellationToken = default);
        Task<OrderDto> UpdateStatusAsync(int id, OrderStatusUpdateDto dto, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task DeleteBulkAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
    }
}