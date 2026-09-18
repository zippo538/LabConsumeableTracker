using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using LabConsumableExpiryTracker.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LabConsumableExpiryTracker.Infrastructure.Repositories;

public class DisposalRepository : Repository<Disposal, Guid>, IDisposalRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<DisposalRepository> _logger;

    public DisposalRepository(AppDbContext context, ILogger<DisposalRepository> logger)
        : base(context)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Disposal>> GetByLotIdAsync(Guid lotId, CancellationToken ct = default)
    {
        _logger.LogDebug("Retrieving disposals for lot {LotId}", lotId);

        return await _context.Disposals
            .AsNoTracking()
            .Where(disposal => disposal.LotId == lotId)
            .OrderByDescending(disposal => disposal.DisposedAt)
            .ToListAsync(ct);
    }
    
    public async Task<IReadOnlyList<Disposal>> SearchAsync(
        Guid? itemId, Guid? lotId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct)
    {
        var query = _context.Disposals
            .AsNoTracking()
            .Include(d => d.Lot)
            .ThenInclude(l => l.Item)   // ← ini yang kemungkinan hilang/kurang
            .AsQueryable();

        if (lotId is not null) query = query.Where(d => d.LotId == lotId);
        if (itemId is not null) query = query.Where(d => d.Lot.ItemId == itemId);
        if (from is not null) query = query.Where(d => d.DisposedAt >= from);
        if (to is not null) query = query.Where(d => d.DisposedAt <= to);

        return await query
            .OrderByDescending(d => d.DisposedAt)
            .ThenBy(d => d.Id)
            .ToListAsync(ct);
    }
}
