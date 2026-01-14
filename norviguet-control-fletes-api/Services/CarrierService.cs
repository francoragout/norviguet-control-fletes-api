using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
                ?? throw new KeyNotFoundException("Carrier not found");

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
                ?? throw new KeyNotFoundException("Carrier not found");
            mapper.Map(dto, carrier);
            await context.SaveChangesAsync();
            return mapper.Map<CarrierDto>(carrier);
        }

        public async Task DeleteAsync(int id)
        {
            var carrier = await context.Carriers
                .FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new KeyNotFoundException("Carrier not found");
            context.Carriers.Remove(carrier);
            await context.SaveChangesAsync();
        }

        public async Task DeleteBulkAsync(IEnumerable<int> ids)
        {
            await context.Carriers
                .Where(x => ids.Contains(x.Id))
                .ExecuteDeleteAsync();
        }
    }
}
