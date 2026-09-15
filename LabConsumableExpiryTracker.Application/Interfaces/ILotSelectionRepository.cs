using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Application.Interfaces;

public interface ILotSelectionRepository
{
    Task<IReadOnlyList<Lot>> GetAvailableLotsAsync(
        Guid itemId,
        DateTimeOffset now,
        CancellationToken ct);
}