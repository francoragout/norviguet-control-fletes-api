using norviguet_control_fletes_api.Models.DTOs.Common;
using norviguet_control_fletes_api.Models.DTOs.Order;

namespace norviguet_control_fletes_api.Services.Interfaces
{
    public interface IOrderService
    {
        Task<PagedResultDto<OrderDto>> GetAllAsync(PagedRequestDto dto, CancellationToken cancellationToken);
        Task<OrderDto> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<OrderDto> CreateAsync(OrderCreateDto dto, CancellationToken cancellationToken);
        Task<OrderDto> UpdateAsync(int id, OrderUpdateDto dto, CancellationToken cancellationToken);
        Task<OrderDto> UpdateStatusAsync(int id, OrderStatusUpdateDto dto, CancellationToken cancellationToken);
        Task DeleteAsync(int id, CancellationToken cancellationToken);
        Task DeleteBulkAsync(IEnumerable<int> ids, CancellationToken cancellationToken);
    }
}