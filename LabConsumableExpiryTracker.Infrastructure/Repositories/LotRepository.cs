using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using LabConsumableExpiryTracker.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LabConsumableExpiryTracker.Infrastructure.Repositories;

public class LotRepository : Repository<Lot, Guid>, ILotRepository
{
    private readonly AppDbContext _context;

    public LotRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(Guid itemId, string lotNumber, DateOnly expiryDate, CancellationToken ct)
    {
        var normalizedLotNumber = lotNumber.Trim();
        return await _context.Lots
        .AsNoTracking()
        .AnyAsync(
           lot =>
               lot.ItemId == itemId &&
               lot.LotNumber == normalizedLotNumber &&
               lot.ExpiryDate == expiryDate,
               ct
           );
    }

    public async Task<IEnumerable<Lot>> GetByItemIdAsync(
        Guid itemId,
        CancellationToken ct = default)
    {
        return await _context.Lots
            .AsNoTracking()
            .Where(lot => lot.ItemId == itemId)
            .OrderBy(lot => lot.ExpiryDate)
            .ThenBy(lot => lot.ReceivedAt)
            .ToListAsync(ct);
    }
    public async Task<decimal> GetTotalUsableQuantityAsync(
        Guid itemId,
        DateOnly today,
        CancellationToken ct = default)
    {
        var total = await _context.Lots
            .Where(lot =>
                lot.ItemId == itemId &&
                lot.Status == LotStatus.Active &&
                lot.RemainingQuantity > 0 &&
                lot.ExpiryDate >= today)
            .SumAsync(
                lot => (decimal?)lot.RemainingQuantity,
                ct);

        return total ?? 0m;
    }

    public async Task<IReadOnlyDictionary<Guid, decimal>> GetUsableQuantityByItemIdsAsync(IEnumerable<Guid> itemIds, DateOnly today, CancellationToken ct = default)
    {
        var ids = itemIds
        .Where(id => id != Guid.Empty)
        .Distinct()
        .ToArray();

        if (ids.Length == 0)
        {
            return new Dictionary<Guid, decimal>();
        }
        return await DbSet
            .Where(lot =>
                ids.Contains(lot.ItemId) &&
                lot.Status == LotStatus.Active &&
                lot.RemainingQuantity > 0 &&
                lot.ExpiryDate >= today)
            .GroupBy(lot => lot.ItemId)
            .ToDictionaryAsync(
                group => group.Key,
                group => group.Sum(lot => lot.RemainingQuantity),
                ct);
    }
}
