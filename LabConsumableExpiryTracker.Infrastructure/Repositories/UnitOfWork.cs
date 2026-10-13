using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Infrastructure.Persistence;

namespace LabConsumableExpiryTracker.Infrastructure.Repositories;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(
        CancellationToken ct = default)
    {
        return await context.SaveChangesAsync(ct);
    }
}