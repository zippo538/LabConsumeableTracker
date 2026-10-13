using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LabConsumableExpiryTracker.Infrastructure.Repositories;

public class LotRepository(
    AppDbContext context,
    ILogger<LotRepository> logger) : Repository<Lot, Guid>(context), ILotRepository
{
    private readonly AppDbContext _context = context;

    public async Task<bool> ExistsAsync(Guid itemId, string lotNumber, DateOnly expiryDate, CancellationToken ct)
    {
        logger.LogDebug(
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

        logger.LogDebug(
            "Lot existence check completed for item {ItemId} with result {Exists}",
            itemId,
            exists);

        return exists;
    }

    public async Task<IReadOnlyList<Lot>> GetAllWithItemAsync(
        CancellationToken ct = default)
    {
        logger.LogDebug("Retrieving all lots with item navigation included");

        var lots = await _context.Lots
            .AsNoTracking()
            .Include(lot => lot.Item)
            .ToListAsync(ct);

        logger.LogDebug("Retrieved {LotCount} lots", lots.Count);

        return lots;
    }

    public async Task<PagedResult<Lot>> GetAllWithItemAsyncPage(
        int page,
        int pageSize,
        string? search = null,
        Guid? itemId = null,
        string? administrativeStatus = null,
        string? expiryCondition = null,
        CancellationToken ct = default)
    {
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(pageSize));

        var query = _context.Lots
            .AsNoTracking()
            .Include(lot => lot.Item)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(lot =>
                lot.LotNumber.ToLower().Contains(searchLower) ||
                lot.Item.Name.ToLower().Contains(searchLower) ||
                lot.Item.Code.ToLower().Contains(searchLower));
        }

        if (itemId.HasValue)
        {
            query = query.Where(lot => lot.ItemId == itemId.Value);
        }

        if (!string.IsNullOrWhiteSpace(administrativeStatus))
        {
            if (Enum.TryParse<LotAdministrativeStatus>(administrativeStatus, true, out var status))
            {
                query = query.Where(lot => lot.AdministrativeStatus == status);
            }
        }

        if (!string.IsNullOrWhiteSpace(expiryCondition))
        {
            if (Enum.TryParse<ExpiryCondition>(expiryCondition, true, out var condition))
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                query = condition switch
                {
                    ExpiryCondition.Expired => query.Where(lot => lot.ExpiryDate <= today),
                    ExpiryCondition.ExpiringSoon => query.Where(lot =>
                        lot.ExpiryDate > today &&
                        lot.ExpiryDate <= today.AddDays(lot.Item.ExpiringSoonDays)),
                    ExpiryCondition.Valid => query.Where(lot =>
                        lot.ExpiryDate > today.AddDays(lot.Item.ExpiringSoonDays)),
                    _ => query
                };
            }
        }

        query = query
            .OrderByDescending(lot => lot.CreatedAt)
            .ThenBy(lot => lot.ExpiryDate)
            .ThenBy(lot => lot.LotNumber);

        var totalCount = await query.CountAsync(ct);
        var lots = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Lot>
        {
            Items = lots,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }


    public async Task<Lot?> GetByIdWithItemAsync(
        Guid id,
        CancellationToken ct = default)
    {
        logger.LogDebug("Retrieving lot {LotId}", id);

        return await _context.Lots
            .AsNoTracking()
            .Include(lot => lot.Item)
            .FirstOrDefaultAsync(lot => lot.Id == id, ct);
    }
    public async Task<IEnumerable<Lot>> GetByItemIdAsync(
        Guid itemId,
        CancellationToken ct = default)
    {
        logger.LogDebug("Retrieving lots for item {ItemId}", itemId);

        var lots = await _context.Lots
            .AsNoTracking()
            .Include(lot => lot.Item)
            .Where(lot => lot.ItemId == itemId)
            .OrderBy(lot => lot.ExpiryDate)
            .ThenBy(lot => lot.ReceivedAt)
            .ToListAsync(ct);

        logger.LogDebug(
            "Retrieved {LotCount} lots for item {ItemId}",
            lots.Count,
            itemId);

        return lots;
    }

    public async Task<Lot?> GetByItemIdAndLotNumberAsync(
        Guid itemId,
        string lotNumber,
        CancellationToken ct = default)
    {
        logger.LogDebug(
            "Retrieving lot {LotNumber} for item {ItemId}",
            lotNumber,
            itemId);

        return await _context.Lots
            .FirstOrDefaultAsync(
                lot => lot.ItemId == itemId && lot.LotNumber == lotNumber,
                ct);
    }

    public async Task<decimal> GetTotalUsableQuantityAsync(
        Guid itemId,
        DateOnly today,
        CancellationToken ct = default)
    {
        logger.LogDebug(
            "Calculating usable quantity for item {ItemId} on {EvaluationDate}",
            itemId,
            today);

        var total = await _context.Lots
            .AsNoTracking()
            .Where(lot =>
                lot.ItemId == itemId &&
                lot.AdministrativeStatus == LotAdministrativeStatus.Active &&
                lot.RemainingQuantity > 0 &&
                lot.ExpiryDate >= today)
            .SumAsync(
                lot => (decimal?)lot.RemainingQuantity,
                ct);

        var usableQuantity = total ?? 0m;

        logger.LogDebug(
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
            logger.LogDebug("Usable quantity query skipped because no valid item IDs were supplied");
            return new Dictionary<Guid, decimal>();
        }

        logger.LogDebug(
            "Calculating usable quantities for {ItemCount} items on {EvaluationDate}",
            ids.Length,
            today);

        var usableQuantities = await DbSet
            .AsNoTracking()
            .Where(lot =>
                ids.Contains(lot.ItemId) &&
                lot.AdministrativeStatus == LotAdministrativeStatus.Active &&
                lot.RemainingQuantity > 0 &&
                lot.ExpiryDate >= today)
            .GroupBy(lot => lot.ItemId)
            .ToDictionaryAsync(
                group => group.Key,
                group => group.Sum(lot => lot.RemainingQuantity),
                ct);

        logger.LogDebug(
            "Usable quantities calculated for {ResultItemCount} of {RequestedItemCount} items",
            usableQuantities.Count,
            ids.Length);

        return usableQuantities;
    }
    public async Task<IReadOnlyList<Lot>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct) =>
        await _context.Lots.AsNoTracking().Include(l => l.Item)
            .Where(l => ids.Contains(l.Id)).ToListAsync(ct);
    public async Task<IReadOnlyList<Lot>> GetAllWithItemByExpiryConditionAsync(
      ExpiryCondition expiryCondition,
      DateOnly evaluationDate,
      CancellationToken ct = default)
    {
        if (!Enum.IsDefined(typeof(ExpiryCondition), expiryCondition))
        {
            throw new ArgumentOutOfRangeException(nameof(expiryCondition));
        }

        logger.LogDebug(
            "Retrieving lots with expiry condition {ExpiryCondition} evaluated on {EvaluationDate}",
            expiryCondition,
            evaluationDate);

        var query = _context.Lots
            .AsNoTracking()
            .Include(lot => lot.Item)
            .AsQueryable();

        query = expiryCondition switch
        {
            ExpiryCondition.Expired => query.Where(lot => lot.ExpiryDate <= evaluationDate),

            ExpiryCondition.ExpiringSoon => query.Where(lot =>
                lot.ExpiryDate > evaluationDate &&
                lot.ExpiryDate <= evaluationDate.AddDays(lot.Item.ExpiringSoonDays)),

            ExpiryCondition.Valid => query.Where(lot =>
                lot.ExpiryDate > evaluationDate.AddDays(lot.Item.ExpiringSoonDays)),

            _ => throw new ArgumentOutOfRangeException(nameof(expiryCondition))
        };

        var lots = await query
            .OrderBy(lot => lot.ExpiryDate)
            .ThenBy(lot => lot.LotNumber)
            .ToListAsync(ct);

        logger.LogDebug(
            "Retrieved {LotCount} lots with expiry condition {ExpiryCondition}",
            lots.Count,
            expiryCondition);

        return lots;
    }
}