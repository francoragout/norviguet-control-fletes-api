using norviguet_control_fletes_api.Models.DTOs.Invoice;

namespace norviguet_control_fletes_api.Services.Interfaces
{
    public interface IInvoiceService
    {
        Task<IReadOnlyList<InvoiceDto>> GetAllAsync(CancellationToken cancellationToken);
        Task<InvoiceDto> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<InvoiceDto> CreateAsync(InvoiceCreateDto dto, CancellationToken cancellationToken);
        Task<InvoiceDto> UpdateAsync(int id, InvoiceUpdateDto dto, CancellationToken cancellationToken);
        Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken);
    }
}