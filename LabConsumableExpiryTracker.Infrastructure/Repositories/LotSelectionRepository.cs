using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LabConsumableExpiryTracker.Infrastructure.Repositories;

public class LotSelectionRepository : ILotSelectionRepository
{
    private readonly AppDbContext _context;

    public LotSelectionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Lot>> GetAvailableLotsAsync(
        Guid itemId,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var lotsForItem = await _context.Lots
            .Where(lot => lot.ItemId == itemId)   
            .ToListAsync(ct);                      

        return lotsForItem
            .Where(lot => lot.IsEligible(now))    
            .OrderBy(lot => lot.ExpiryDate)
            .ThenBy(lot => lot.ReceivedAt)
            .ThenBy(lot => lot.Id)
            .ToList();
    }
}