using norviguet_control_fletes_api.Models.DTOs.PaymentOrder;

namespace norviguet_control_fletes_api.Services.Interfaces
{
    public interface IPaymentOrderService
    {
        Task<IReadOnlyList<PaymentOrderDto>> GetAllAsync(CancellationToken cancellationToken);
        Task<PaymentOrderDto> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<PaymentOrderDto> CreateAsync(PaymentOrderCreateDto dto, CancellationToken cancellationToken);
        Task<PaymentOrderDto> UpdateAsync(int id, PaymentOrderUpdateDto dto, CancellationToken cancellationToken);
        Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken);
    }
}