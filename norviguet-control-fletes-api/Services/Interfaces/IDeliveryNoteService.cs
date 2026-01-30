using norviguet_control_fletes_api.Models.DTOs.DeliveryNote;

namespace norviguet_control_fletes_api.Services.Interfaces
{
    public interface IDeliveryNoteService
    {
        Task<IReadOnlyList<DeliveryNoteDto>> GetAllAsync(CancellationToken cancellationToken);
        Task<DeliveryNoteDto> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<DeliveryNoteDto> CreateAsync(DeliveryNoteCreateDto dto, CancellationToken cancellationToken);
        Task<DeliveryNoteDto> UpdateAsync(int id, DeliveryNoteUpdateDto dto, CancellationToken cancellationToken);
        Task<DeliveryNoteDto> UpdateStatusAsync(int id, DeliveryNoteStatusUpdateDto dto, CancellationToken cancellationToken);
        Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken);
    }
}