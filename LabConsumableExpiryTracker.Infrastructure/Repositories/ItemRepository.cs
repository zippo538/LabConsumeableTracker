using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using LabConsumableExpiryTracker.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LabConsumableExpiryTracker.Infrastructure.Repositories
{
    public class ItemRepository : Repository<Item, Guid>, IItemRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ItemRepository> _logger;

        public ItemRepository(
            AppDbContext context,
            ILogger<ItemRepository> logger) : base(context)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IReadOnlyList<Item>> GetAllWithLotsAsync(CancellationToken ct = default)
        {
            _logger.LogDebug("Retrieving all items with their lots");

            var items = await _context.Items
                .AsNoTracking()
                .Include(item => item.Lots)
                .ToListAsync(ct);

            _logger.LogDebug(
                "Retrieved {ItemCount} items with their lots",
                items.Count);

            return items;
        }

        public async Task<PagedResult<Item>> GetAllWithLotsAsyncPaged(
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            _logger.LogDebug($"Retrieving items of {pageSize}");
            if (page < 0)
                throw new ArgumentOutOfRangeException(nameof(page));

            if (pageSize is < 1 or > 100)
                throw new ArgumentOutOfRangeException(nameof(pageSize));

            var query = _context.Items
                .AsNoTracking()
                .Include(item => item.Lots)
                .OrderBy(item => item.Code);

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
            _logger.LogDebug($"Querry success retrive {items} with total count {totalCount}");

            return new PagedResult<Item>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<Item?> GetByCodeAsync(
            string code,
            CancellationToken ct = default)
        {
            _logger.LogDebug("Retrieving item by code {ItemCode}", code);

            var item = await _context.Items
                .FirstOrDefaultAsync(
                    item => item.Code == code,
                    ct);

            _logger.LogDebug(
                "Item lookup by code {ItemCode} completed with result {ItemFound}",
                code,
                item is not null);

            return item;
        }

        public async Task<Item?> GetByIdWithLotsAsync(Guid id, CancellationToken ct = default)
        {
            _logger.LogDebug("Retrieving item {ItemId} with its lots", id);

            var item = await _context.Items
                .Include(item => item.Lots)
                .FirstOrDefaultAsync(item => item.Id == id, ct);

            _logger.LogDebug(
                "Item {ItemId} lookup with lots completed with result {ItemFound}",
                id,
                item is not null);

            return item;
        }
    }
}
