using LabConsumableExpiryTracker.Application.DTOs.ConsumptionHistoryDTOs;

namespace LabConsumableExpiryTracker.Application.Interfaces;

public interface IConsumptionHistoryRepository
{
    Task<(IReadOnlyList<ConsumptionListItemDto> Items, int TotalCount)> GetPagedAsync(
        Guid consumedBy,
        Guid? jobId,
        Guid? itemId,
        Guid? lotId,
        string? lotNumber,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int page,
        int pageSize,
        CancellationToken ct = default);
    
    Task<ConsumptionHistoryDetailDto?> GetDetailAsync(
        Guid consumptionId,
        Guid consumedBy,
        CancellationToken ct = default);
}