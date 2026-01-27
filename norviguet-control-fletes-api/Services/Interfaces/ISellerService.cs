using norviguet_control_fletes_api.Models.DTOs.Common;
using norviguet_control_fletes_api.Models.DTOs.Seller;

namespace norviguet_control_fletes_api.Services.Interfaces
{
    public interface ISellerService
    {
        Task<PagedResultDto<SellerDto>> GetAllAsync(PagedRequestDto dto, CancellationToken cancellationToken);
        Task<SellerDto> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<SellerDto> CreateAsync(SellerCreateDto dto, CancellationToken cancellationToken);
        Task<SellerDto> UpdateAsync(int id, SellerUpdateDto dto, CancellationToken cancellationToken);
        Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken);
    }
}