using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using LabConsumableExpiryTracker.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LabConsumableExpiryTracker.Infrastructure.Repositories
{
    public class ItemRepository : Repository<Item, Guid>, IItemRepository
    {
        private readonly AppDbContext _context;
        public ItemRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<IReadOnlyList<Item>> GetAllWithLotsAsync(CancellationToken ct = default)
        {
            return await _context.Items
            .AsNoTracking()
            .Include(item => item.Lots)
            .ToListAsync(ct);
        }

        public async Task<Item?> GetByCodeAsync(
       string code,
       CancellationToken ct = default)
        {
            return await _context.Items
                .FirstOrDefaultAsync(
                    item => item.Code == code,
                    ct);
        }

        public async Task<Item?> GetByIdWithLotsAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Items
            .Include(item => item.Lots)
            .FirstOrDefaultAsync(item => item.Id == id, ct);
        }


    }
}