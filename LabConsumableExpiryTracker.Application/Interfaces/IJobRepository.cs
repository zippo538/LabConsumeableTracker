
using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Application.Interfaces;

public interface IJobRepository : IRepository<Job, Guid>
{
    Task<bool> ExistsByJobNumberAsync(string jobNumber, CancellationToken ct = default);
}