using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Common.Middlewares;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Models.DTOs.Common;
using norviguet_control_fletes_api.Models.Entities;

namespace norviguet_control_fletes_api.Services
{
    public abstract class BaseService<TEntity, TDto>(ApplicationDbContext context, IMapper mapper)
        where TEntity : AuditableEntity
    {
        protected readonly ApplicationDbContext Context = context;
        protected readonly IMapper Mapper = mapper;

        protected abstract DbSet<TEntity> DbSet { get; }
        protected abstract string EntityName { get; }
        protected abstract string EntityNamePlural { get; }

        public async Task<TDto> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var entity = await DbSet
                .AsNoTracking()
                .Where(x => x.Id == id)
                .ProjectTo<TDto>(Mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException($"{EntityName} not found");

            return entity;
        }

        public async Task<PagedResultDto<TDto>> GetAllAsync(PagedRequestDto dto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var query = DbSet
                .AsNoTracking()
                .OrderByDescending(e => e.CreatedAt);

            var totalItems = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip(dto.GetSkip())
                .Take(dto.PageSize)
                .ProjectTo<TDto>(Mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return new PagedResultDto<TDto>
            {
                Items = items,
                Page = dto.Page,
                PageSize = dto.PageSize,
                TotalItems = totalItems
            };
        }

        protected async Task<TEntity> GetEntityByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await DbSet
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
                ?? throw new NotFoundException($"{EntityName} not found");
        }

        protected async Task SaveWithConcurrencyAsync(TEntity entity, byte[] rowVersion, CancellationToken cancellationToken)
        {
            Context.Entry(entity).Property(e => e.RowVersion).OriginalValue = rowVersion;

            try
            {
                await Context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException(
                    "The record was modified by another user. Please reload and try again.");
            }
        }

        public async Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(ids);

            var idList = ids.Distinct().ToList();

            if (idList.Count == 0) return;

            var existingIds = await DbSet
                .Where(e => idList.Contains(e.Id))
                .Select(e => e.Id)
                .ToListAsync(cancellationToken);

            if (existingIds.Count != idList.Count)
                throw new NotFoundException($"Some of the specified {EntityNamePlural} were not found");

            await ValidateDeleteConstraintsAsync(idList, cancellationToken);

            await DbSet
                .Where(e => idList.Contains(e.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }

        protected virtual Task ValidateDeleteConstraintsAsync(List<int> ids, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
