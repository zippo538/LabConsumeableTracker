
using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Application.Interfaces
{
    public interface IJobRepository : IRepository<Job, Guid>
    {
        Task<Job?> GetByJobNumberAsync(string jobNumber, CancellationToken ct = default);
        Task<bool> ExistsByJobNumberAsync(string jobNumber, CancellationToken ct = default);
    }
}