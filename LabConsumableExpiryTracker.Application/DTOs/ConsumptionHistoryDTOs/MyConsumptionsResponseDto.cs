
namespace LabConsumableExpiryTracker.Application.DTOs.ConsumptionHistoryDTOs;

public class MyConsumptionsResponseDto
{
    public IReadOnlyList<ConsumptionListItemDto> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}