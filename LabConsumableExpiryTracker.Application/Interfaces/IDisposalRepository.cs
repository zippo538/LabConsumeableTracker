using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Application.Interfaces;

public interface IDisposalRepository : IRepository<Disposal, Guid>
{
    Task<IReadOnlyList<Disposal>> SearchAsync(
        Guid? itemId,
        Guid? lotId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken ct);
}
