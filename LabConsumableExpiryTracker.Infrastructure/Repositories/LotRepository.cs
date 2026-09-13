using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using LabConsumableExpiryTracker.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LabConsumableExpiryTracker.Infrastructure.Repositories;

public class LotRepository : Repository<Lot, Guid>, ILotRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<LotRepository> _logger;

    public LotRepository(
        AppDbContext context,
        ILogger<LotRepository> logger) : base(context)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> ExistsAsync(Guid itemId, string lotNumber, DateOnly expiryDate, CancellationToken ct)
    {
        _logger.LogDebug(
            "Checking whether a lot exists for item {ItemId} with expiry date {ExpiryDate}",
            itemId,
            expiryDate);

        var normalizedLotNumber = lotNumber.Trim();
        var exists = await _context.Lots
            .AsNoTracking()
            .AnyAsync(
                lot =>
                    lot.ItemId == itemId &&
                    lot.LotNumber == normalizedLotNumber &&
                    lot.ExpiryDate == expiryDate,
                ct);

        _logger.LogDebug(
            "Lot existence check completed for item {ItemId} with result {Exists}",
            itemId,
            exists);

        return exists;
    }
    
    public new async Task<IReadOnlyList<Lot>> GetAllWithItemAsync(
        CancellationToken ct = default)
    {
        _logger.LogDebug("Retrieving all lots with item navigation included");

        var lots = await _context.Lots
            .AsNoTracking()
            .Include(lot => lot.Item)
            .ToListAsync(ct);

        _logger.LogDebug("Retrieved {LotCount} lots", lots.Count);

        return lots;
    }

    public async Task<Lot?> GetByIdWithItemAsync(
        Guid id,
        CancellationToken ct = default)
    {
        _logger.LogDebug("Retrieving lot {LotId}", id);

        return await _context.Lots
            .AsNoTracking()
            .Include(lot => lot.Item)
            .FirstOrDefaultAsync(lot => lot.Id == id, ct);
    }
    public async Task<IEnumerable<Lot>> GetByItemIdAsync(
        Guid itemId,
        CancellationToken ct = default)
    {
        _logger.LogDebug("Retrieving lots for item {ItemId}", itemId);

        var lots = await _context.Lots
            .AsNoTracking()
            .Include(lot => lot.Item)
            .Where(lot => lot.ItemId == itemId)
            .OrderBy(lot => lot.ExpiryDate)
            .ThenBy(lot => lot.ReceivedAt)
            .ToListAsync(ct);

        _logger.LogDebug(
            "Retrieved {LotCount} lots for item {ItemId}",
            lots.Count,
            itemId);

        return lots;
    }

    public async Task<decimal> GetTotalUsableQuantityAsync(
        Guid itemId,
        DateOnly today,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "Calculating usable quantity for item {ItemId} on {EvaluationDate}",
            itemId,
            today);

        var total = await _context.Lots
            .AsNoTracking()
            .Where(lot =>
                lot.ItemId == itemId &&
                lot.Status == LotStatus.Active &&
                lot.RemainingQuantity > 0 &&
                lot.ExpiryDate >= today)
            .SumAsync(
                lot => (decimal?)lot.RemainingQuantity,
                ct);

        var usableQuantity = total ?? 0m;

        _logger.LogDebug(
            "Usable quantity calculated for item {ItemId}: {UsableQuantity}",
            itemId,
            usableQuantity);

        return usableQuantity;
    }

    public async Task<IReadOnlyDictionary<Guid, decimal>> GetUsableQuantityByItemIdsAsync(IEnumerable<Guid> itemIds,
        DateOnly today, CancellationToken ct = default)
    {
        var ids = itemIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        if (ids.Length == 0)
        {
            _logger.LogDebug("Usable quantity query skipped because no valid item IDs were supplied");
            return new Dictionary<Guid, decimal>();
        }

        _logger.LogDebug(
            "Calculating usable quantities for {ItemCount} items on {EvaluationDate}",
            ids.Length,
            today);

        var usableQuantities = await DbSet
            .AsNoTracking()
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

        _logger.LogDebug(
            "Usable quantities calculated for {ResultItemCount} of {RequestedItemCount} items",
            usableQuantities.Count,
            ids.Length);

        return usableQuantities;
    }
}