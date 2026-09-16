using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Application.Services;

public class LotSelectionService : ILotSelectionService
{
    private readonly ILotSelectionRepository _lotSelectionRepository;

    public LotSelectionService(
        ILotSelectionRepository lotSelectionRepository)
    {
        _lotSelectionRepository = lotSelectionRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<LotAllocation>>> AllocateAsync(
        Guid itemId,
        decimal requestedQuantity,
        DateTimeOffset now,
        CancellationToken ct)
    {
        if (requestedQuantity <= 0)
        {
            return ServiceResult<IReadOnlyList<LotAllocation>>.ErrorResult(
                "Requested quantity must be greater than zero.");
        }

        var eligibleLots =
            await _lotSelectionRepository.GetAvailableLotsAsync(
                itemId,
                now,
                ct);

        var totalEligible = eligibleLots.Sum(
            lot => lot.RemainingQuantity);

        if (totalEligible < requestedQuantity)
        {
            return ServiceResult<IReadOnlyList<LotAllocation>>.ErrorResult(
                "Insufficient eligible stock to fulfill the requested quantity.");
        }

        var allocations = new List<LotAllocation>();
        var remaining = requestedQuantity;

        foreach (var lot in eligibleLots)
        {
            if (remaining <= 0)
                break;

            var take = Math.Min(
                lot.RemainingQuantity,
                remaining);

            allocations.Add(
                new LotAllocation(lot.Id, take));

            remaining -= take;
        }

        return ServiceResult<IReadOnlyList<LotAllocation>>.SuccessResult(
            allocations,
            "Lots allocated successfully.");
    }
}