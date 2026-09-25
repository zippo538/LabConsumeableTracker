using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;
using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Application.Interfaces;

public interface ILotRepository : IRepository<Lot, Guid>
{
    Task<IReadOnlyList<Lot>> GetAllWithItemAsync(CancellationToken ct = default);
    Task<PagedResult<Lot>> GetAllWithItemAsyncPage(
        int page,
        int pageSize,
        string? search = null,
        Guid? itemId = null,
        string? administrativeStatus = null,
        string? expiryCondition = null,
        CancellationToken ct = default);

    Task<Lot?> GetByIdWithItemAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Lot>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct);
    Task<IEnumerable<Lot>> GetByItemIdAsync(Guid itemId, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid itemId, string lotNumber, DateOnly expiryDate, CancellationToken ct);
    Task<decimal> GetTotalUsableQuantityAsync(Guid itemId, DateOnly today, CancellationToken ct = default);
    Task<IReadOnlyDictionary<Guid, decimal>> GetUsableQuantityByItemIdsAsync(IEnumerable<Guid> itemIds, DateOnly today, CancellationToken ct = default);
}