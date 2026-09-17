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
}
