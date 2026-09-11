using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Infrastructure.Persistence;

namespace LabConsumableExpiryTracker.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }
        public async Task<int> SaveChangesAsync(
        CancellationToken ct = default)
        {
            return await _context.SaveChangesAsync(ct);
        }
    }
}