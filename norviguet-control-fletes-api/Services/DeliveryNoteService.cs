using AutoMapper;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.DeliveryNote;
using norviguet_control_fletes_api.Services.Interfaces;

namespace norviguet_control_fletes_api.Services
{
    public class DeliveryNoteService(ApplicationDbContext context, IMapper mapper) : IDeliveryNoteService
    {
        public async Task<DeliveryNoteDto> GetAllAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<DeliveryNoteDto> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<DeliveryNoteDto> CreateAsync(DeliveryNoteCreateDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<DeliveryNoteDto> UpdateAsync(int id, DeliveryNoteUpdateDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
