using AutoMapper;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.Common;
using norviguet_control_fletes_api.Models.DTOs.Seller;
using norviguet_control_fletes_api.Services.Interfaces;

namespace norviguet_control_fletes_api.Services
{
    public class SellerService(ApplicationDbContext context, IMapper mapper) : ISellerService
    {
        public async Task<PagedResultDto<SellerDto>> GetSellerInfoAsync(int sellerId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<SellerDto> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<SellerDto> CreateAsync(SellerCreateDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<SellerDto> UpdateAsync(int id, SellerUpdateDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
