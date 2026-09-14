using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Domain.Services.Interfaces;


namespace LabConsumableExpiryTracker.Domain.Services;
public class LotSelectionService : ILotSelectionService
{
    public IReadOnlyList<LotAllocation> Allocate(
        IReadOnlyList<Lot> lots,
        decimal requestedQuantity,
        DateTimeOffset now)
    {
        if (requestedQuantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedQuantity),
                "Requested quantity must be greater than zero.");
        }

        var eligible = lots
            .Where(lot => lot.IsEligible(now))       
            .OrderBy(lot => lot.ExpiryDate)          
            .ThenBy(lot => lot.ReceivedAt)            
            .ThenBy(lot => lot.Id)                    
            .ToList();

        var totalEligible = eligible.Sum(lot => lot.RemainingQuantity);
        if (totalEligible < requestedQuantity)
        {
            throw new InvalidOperationException(
                "Insufficient eligible stock to fulfill the requested quantity.");
        }
        
        var allocations = new List<LotAllocation>();
        var remaining = requestedQuantity;

        foreach (var lot in eligible)
        {
            if (remaining <= 0)
            {
                break;
            }

            var take = Math.Min(lot.RemainingQuantity, remaining);
            allocations.Add(new LotAllocation(lot.Id, take));
            remaining -= take;
        }

        return allocations;
    }
}