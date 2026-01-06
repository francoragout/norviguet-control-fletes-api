using norviguet_control_fletes_api.Entities;

namespace norviguet_control_fletes_api.Repositories
{
    public interface ICarrierRepository
    {
        Task<List<Carrier>> GetAllAsync();
        Task<Carrier?> GetByIdAsync(int id);
        Task<Carrier?> GetByIdWithRelationsAsync(int id);
        Task<List<Carrier>> GetByIdsWithRelationsAsync(List<int> ids);
        Task<List<Carrier>> GetCarriersWithoutInvoicesByOrderIdAsync(int orderId);
        Task<List<Carrier>> GetCarriersWithoutPaymentOrdersByOrderIdAsync(int orderId);
        Task AddAsync(Carrier carrier);
        Task UpdateAsync(Carrier carrier);
        Task DeleteAsync(Carrier carrier);
        Task DeleteRangeAsync(List<Carrier> carriers);
        Task<bool> ExistsAsync(int id);
        Task<int> SaveChangesAsync();
    }
}
