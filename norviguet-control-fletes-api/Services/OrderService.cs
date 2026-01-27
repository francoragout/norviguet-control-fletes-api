using AutoMapper;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.Common;
using norviguet_control_fletes_api.Models.DTOs.Order;
using norviguet_control_fletes_api.Services.Interfaces;

namespace norviguet_control_fletes_api.Services
{
    public class OrderService(ApplicationDbContext context, IMapper mapper) : IOrderService
    {
        public async Task<PagedResultDto<OrderDto>> GetAllAsync(PagedRequestDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<OrderDto> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<OrderDto> CreateAsync(OrderCreateDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<OrderDto> UpdateAsync(int id, OrderUpdateDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<OrderDto> UpdateStatusAsync(int id, OrderStatusUpdateDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
