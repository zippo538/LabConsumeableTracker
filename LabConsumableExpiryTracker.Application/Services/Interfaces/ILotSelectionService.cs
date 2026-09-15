using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Domain.Models;

namespace LabConsumableExpiryTracker.Application.Services.Interfaces;

public interface ILotSelectionService
{
    Task<ServiceResult<IReadOnlyList<LotAllocation>>> AllocateAsync(
        Guid itemId,
        decimal requestedQuantity,
        DateTimeOffset now,
        CancellationToken cancellationToken);
}