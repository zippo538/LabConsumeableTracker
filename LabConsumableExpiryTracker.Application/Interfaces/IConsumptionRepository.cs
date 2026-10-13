using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Application.Interfaces;

public interface IConsumptionRepository : IRepository<Consumption, Guid>
{
    Task<IReadOnlyList<Consumption>> SearchAsync(
        Guid? jobId,
        Guid? itemId,
        Guid? lotId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken ct);

    Task<Consumption?> GetPendingByJobAndLotAsync(Guid jobId, Guid lotId, CancellationToken ct);
}