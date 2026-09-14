using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Domain.Services.Interfaces;

public interface ILotSelectionService
{
    IReadOnlyList<LotAllocation> Allocate(
        IReadOnlyList<Lot> lots,
        decimal requestedQuantity,
        DateTimeOffset now);
}