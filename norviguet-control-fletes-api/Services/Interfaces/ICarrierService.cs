using norviguet_control_fletes_api.Models.DTOs.Carrier;

namespace norviguet_control_fletes_api.Services.Interfaces
{
    public interface ICarrierService
    {
        Task<IReadOnlyList<CarrierDto>> GetAllAsync();
        Task<CarrierDto> GetByIdAsync(int id);
        Task<CarrierDto> CreateAsync(CarrierCreateDto dto);
        Task<CarrierDto> UpdateAsync(int id, CarrierUpdateDto dto);
        Task DeleteAsync(int id);
        Task DeleteBulkAsync(IEnumerable<int> ids);
    }
}