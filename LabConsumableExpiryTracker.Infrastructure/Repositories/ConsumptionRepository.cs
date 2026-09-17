using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using LabConsumableExpiryTracker.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LabConsumableExpiryTracker.Infrastructure.Repositories;

public class ConsumptionRepository : Repository<Consumption, Guid>, IConsumptionRepository
{
    private readonly AppDbContext _context;
    public ConsumptionRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }
    public async Task<IReadOnlyList<Consumption>> SearchAsync(
        Guid? jobId, Guid? itemId, Guid? lotId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct)
    {
        var query = _context.Consumptions.AsNoTracking().AsQueryable();

        if (jobId is not null) query = query.Where(c => c.JobId == jobId);
        if (lotId is not null) query = query.Where(c => c.LotId == lotId);
        if (from is not null) query = query.Where(c => c.ConsumedAt >= from);
        if (to is not null) query = query.Where(c => c.ConsumedAt <= to);

        if (itemId is not null)
        {
            var lotIds = await _context.Lots
                .Where(l => l.ItemId == itemId)
                .Select(l => l.Id)
                .ToListAsync(ct);
            query = query.Where(c => lotIds.Contains(c.LotId));
        }

        return await query
            .OrderByDescending(c => c.ConsumedAt)
            .ThenBy(c => c.Id)
            .ToListAsync(ct);
    }
    
}
