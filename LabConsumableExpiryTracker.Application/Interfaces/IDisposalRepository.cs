using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Application.Interfaces;

public interface IDisposalRepository : IRepository<Disposal, Guid>
{
    Task<IReadOnlyList<Disposal>> GetByLotIdAsync(Guid lotId, CancellationToken ct = default);
}
