using AutoMapper;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Common.Middlewares;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.Carrier;
using norviguet_control_fletes_api.Models.Entities;
using norviguet_control_fletes_api.Services.Interfaces;

namespace norviguet_control_fletes_api.Services
{
    public class CarrierService(ApplicationDbContext context, IMapper mapper) : ICarrierService
    {
        public async Task<IReadOnlyList<CarrierDto>> GetAllAsync()
        {
            var carriers = await context.Carriers
                .AsNoTracking()
                .ToListAsync();
            return mapper.Map<IReadOnlyList<CarrierDto>>(carriers);
        }

        public async Task<CarrierDto> GetByIdAsync(int id)
        {
            var carrier = await context.Carriers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException("Carrier not found");
            return mapper.Map<CarrierDto>(carrier);
        }

        public async Task<CarrierDto> CreateAsync(CarrierCreateDto dto)
        {
            var carrier = mapper.Map<Carrier>(dto);
            context.Carriers.Add(carrier);
            await context.SaveChangesAsync();
            return mapper.Map<CarrierDto>(carrier);
        }

        public async Task<CarrierDto> UpdateAsync(int id, CarrierUpdateDto dto)
        {
            var carrier = await context.Carriers
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException("Carrier not found");
            mapper.Map(dto, carrier);
            await context.SaveChangesAsync();
            return mapper.Map<CarrierDto>(carrier);
        }

        public async Task DeleteAsync(int id)
        {
            var carrier = await context.Carriers
                .FirstOrDefaultAsync(x => x.Id == id)
               ?? throw new NotFoundException("Carrier not found");
            var hasDeliveryNotes = await context.DeliveryNotes
                .AnyAsync(dn => dn.CarrierId == id);
            if (hasDeliveryNotes)
                throw new ConflictException("Cannot delete carrier because it is associated with one or more delivery notes");
            var hasInvoices = await context.Invoices
                .AnyAsync(i => i.CarrierId == id);
            if (hasInvoices)
                throw new ConflictException("Cannot delete carrier because it is associated with one or more invoices");
            var hasPaymentOrders = await context.PaymentOrders
                .AnyAsync(po => po.CarrierId == id);
            if (hasPaymentOrders)
                throw new ConflictException("Cannot delete carrier because it is associated with one or more payment orders");
            context.Carriers.Remove(carrier);
            await context.SaveChangesAsync();
        }

        public async Task DeleteBulkAsync(IEnumerable<int> ids)
        {
            var carrierIds = ids.ToList();
            var hasDeliveryNotes = await context.DeliveryNotes
                .AnyAsync(dn => carrierIds.Contains(dn.CarrierId));
            if (hasDeliveryNotes)
                throw new ConflictException("Cannot delete one or more carriers because they are associated with delivery notes");
            var hasInvoices = await context.Invoices
                .AnyAsync(i => carrierIds.Contains(i.CarrierId));
            if (hasInvoices)
                throw new ConflictException("Cannot delete one or more carriers because they are associated with invoices");
            var hasPaymentOrders = await context.PaymentOrders
                .AnyAsync(po => carrierIds.Contains(po.CarrierId));
            if (hasPaymentOrders)
                throw new ConflictException("Cannot delete one or more carriers because they are associated with payment orders");
            await context.Carriers
                .Where(x => carrierIds.Contains(x.Id))
                .ExecuteDeleteAsync();
        }
    }
}
