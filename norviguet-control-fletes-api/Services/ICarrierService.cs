using norviguet_control_fletes_api.Models.Carrier;

namespace norviguet_control_fletes_api.Services
{
    public interface ICarrierService
    {
        Task<Result<List<CarrierDto>>> GetAllCarriersAsync();
        Task<Result<CarrierDto>> GetCarrierByIdAsync(int id);
        Task<Result<CarrierDto>> CreateCarrierAsync(CreateCarrierDto dto);
        Task<Result<CarrierDto>> UpdateCarrierAsync(int id, UpdateCarrierDto dto);
        Task<Result<bool>> DeleteCarrierAsync(int id);
        Task<Result<bool>> DeleteCarriersBulkAsync(List<int> ids);
        Task<Result<List<CarrierDto>>> GetCarriersWithoutInvoicesByOrderIdAsync(int orderId);
        Task<Result<List<CarrierDto>>> GetCarriersWithoutPaymentOrdersByOrderIdAsync(int orderId);
    }
}
