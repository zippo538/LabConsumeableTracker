using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Application.Interfaces
{
    public interface IItemRepository : IRepository<Item, Guid>
    {
        Task<Item?> GetByCodeAsync(string code, CancellationToken ct = default);
        Task<IReadOnlyList<Item>> GetAllWithLotsAsync(CancellationToken ct = default);
        Task<Item?> GetByIdWithLotsAsync(Guid id, CancellationToken ct = default);
    }
}