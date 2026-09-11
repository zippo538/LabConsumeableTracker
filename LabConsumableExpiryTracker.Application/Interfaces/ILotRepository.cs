using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Application.Interfaces;

public interface ILotRepository : IRepository<Lot, Guid>
{
    Task<IEnumerable<Lot>> GetByItemIdAsync(Guid itemId, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid itemId, string lotNumber, DateOnly expiryDate, CancellationToken ct);
    Task<decimal> GetTotalUsableQuantityAsync(Guid itemId, DateOnly today, CancellationToken ct = default);
    Task<IReadOnlyDictionary<Guid, decimal>> GetUsableQuantityByItemIdsAsync(IEnumerable<Guid> itemIds, DateOnly today, CancellationToken ct = default);
}

