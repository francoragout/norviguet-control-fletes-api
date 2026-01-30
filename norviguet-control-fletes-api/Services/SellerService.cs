using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Common.Middlewares;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.Common;
using norviguet_control_fletes_api.Models.DTOs.Seller;
using norviguet_control_fletes_api.Models.Entities;
using norviguet_control_fletes_api.Services.Interfaces;

namespace norviguet_control_fletes_api.Services
{
    public class SellerService(ApplicationDbContext context, IMapper mapper) : ISellerService
    {
        public async Task<PagedResultDto<SellerDto>> GetAllAsync(PagedRequestDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var query = context.Sellers
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt);

            var totalItems = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip(dto.GetSkip())
                .Take(dto.PageSize)
                .ProjectTo<SellerDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return new PagedResultDto<SellerDto>
            {
                Items = items,
                Page = dto.Page,
                PageSize = dto.PageSize,
                TotalItems = totalItems
            };
        }

        public async Task<SellerDto> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var seller = await context.Sellers
                .AsNoTracking()
                .Where(x => x.Id == id)
                .ProjectTo<SellerDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException("Seller not found");

            return seller;
        }

        public async Task<SellerDto> CreateAsync(SellerCreateDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var seller = mapper.Map<Seller>(dto);
            context.Sellers.Add(seller);
            await context.SaveChangesAsync(cancellationToken);
            return mapper.Map<SellerDto>(seller);
        }

        public async Task<SellerDto> UpdateAsync(int id, SellerUpdateDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var seller = await context.Sellers
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
                ?? throw new NotFoundException("Seller not found");

            mapper.Map(dto, seller);

            try
            {
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException(
                    "The record was modified by another user. Please reload and try again.");
            }

            return mapper.Map<SellerDto>(seller);
        }

        public async Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(ids);

            var idList = ids.Distinct().ToList();

            if (idList.Count == 0) return;

            var existingIds = await context.Sellers
                .Where(c => idList.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);

            if (existingIds.Count != idList.Count)
                throw new NotFoundException("Some of the specified carriers were not found");

            var conflictIds = await context.Orders
                .Where(o => idList.Contains(o.SellerId))
                .Select(o => o.SellerId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (conflictIds.Any())
            {
                throw new ConflictException(
                    $"Operation aborted. {conflictIds.Count} seller(s) cannot be deleted due to existing orders.");
            }

            await context.Sellers
                .Where(c => idList.Contains(c.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
