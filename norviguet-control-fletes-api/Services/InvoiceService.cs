using AutoMapper;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.Invoice;
using norviguet_control_fletes_api.Services.Interfaces;

namespace norviguet_control_fletes_api.Services
{
    public class InvoiceService(ApplicationDbContext context, IMapper mapper) : IInvoiceService
    {
        public async Task<InvoiceDto> GetAllAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<InvoiceDto> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<InvoiceDto> CreateAsync(InvoiceCreateDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<InvoiceDto> UpdateAsync(int id, InvoiceUpdateDto dto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
