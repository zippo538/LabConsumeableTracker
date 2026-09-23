
namespace LabConsumableExpiryTracker.Application.DTOs.ConsumptionHistoryDTOs;

public class MyConsumptionsResponseDto
{
    public IReadOnlyList<ConsumptionListItemDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}