using norviguet_control_fletes_api.Models.DTOs.Carrier;
using norviguet_control_fletes_api.Models.DTOs.Common;

namespace norviguet_control_fletes_api.Services.Interfaces
{
    public interface ICarrierService
    {
        Task<PagedResultDto<CarrierDto>> GetAllAsync(PagedRequestDto dto, CancellationToken cancellationToken);
        Task<CarrierDto> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<CarrierDto> CreateAsync(CarrierCreateDto dto, CancellationToken cancellationToken);
        Task<CarrierDto> UpdateAsync(int id, CarrierUpdateDto dto, CancellationToken cancellationToken);
        Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken);
    }
}