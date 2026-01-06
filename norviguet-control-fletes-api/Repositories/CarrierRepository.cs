using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Entities;

namespace norviguet_control_fletes_api.Repositories
{
    public class CarrierRepository : ICarrierRepository
    {
        private readonly NorviguetDbContext _context;

        public CarrierRepository(NorviguetDbContext context)
        {
            _context = context;
        }

        public async Task<List<Carrier>> GetAllAsync()
        {
            return await _context.Carriers
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Carrier?> GetByIdAsync(int id)
        {
            return await _context.Carriers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Carrier?> GetByIdWithRelationsAsync(int id)
        {
            return await _context.Carriers
                .Include(c => c.DeliveryNotes)
                .Include(c => c.PaymentOrders)
                .Include(c => c.Invoices)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Carrier>> GetByIdsWithRelationsAsync(List<int> ids)
        {
            return await _context.Carriers
                .Where(c => ids.Contains(c.Id))
                .Include(c => c.DeliveryNotes)
                .Include(c => c.PaymentOrders)
                .Include(c => c.Invoices)
                .ToListAsync();
        }

        public async Task<List<Carrier>> GetCarriersWithoutInvoicesByOrderIdAsync(int orderId)
        {
            var carriersWithDeliveryNotes = await _context.DeliveryNotes
                .Where(dn => dn.OrderId == orderId)
                .Select(dn => dn.Carrier)
                .Distinct()
                .AsNoTracking()
                .ToListAsync();

            var carrierIdsWithInvoice = await _context.Invoices
                .Where(i => i.OrderId == orderId)
                .Select(i => i.CarrierId)
                .Distinct()
                .ToListAsync();

            return carriersWithDeliveryNotes
                .Where(c => !carrierIdsWithInvoice.Contains(c.Id))
                .ToList();
        }

        public async Task<List<Carrier>> GetCarriersWithoutPaymentOrdersByOrderIdAsync(int orderId)
        {
            var carriersWithDeliveryNotes = await _context.DeliveryNotes
                .Where(dn => dn.OrderId == orderId)
                .Select(dn => dn.Carrier)
                .Distinct()
                .AsNoTracking()
                .ToListAsync();

            var carrierIdsWithPaymentOrder = await _context.PaymentOrders
                .Where(po => po.OrderId == orderId)
                .Select(po => po.CarrierId)
                .Distinct()
                .ToListAsync();

            return carriersWithDeliveryNotes
                .Where(c => !carrierIdsWithPaymentOrder.Contains(c.Id))
                .ToList();
        }

        public async Task AddAsync(Carrier carrier)
        {
            await _context.Carriers.AddAsync(carrier);
        }

        public async Task UpdateAsync(Carrier carrier)
        {
            _context.Carriers.Update(carrier);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Carrier carrier)
        {
            _context.Carriers.Remove(carrier);
            await Task.CompletedTask;
        }

        public async Task DeleteRangeAsync(List<Carrier> carriers)
        {
            _context.Carriers.RemoveRange(carriers);
            await Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Carriers.AnyAsync(c => c.Id == id);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
