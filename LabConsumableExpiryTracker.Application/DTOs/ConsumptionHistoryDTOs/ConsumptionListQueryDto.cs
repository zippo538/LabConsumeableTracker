namespace LabConsumableExpiryTracker.Application.DTOs.ConsumptionHistoryDTOs;

public class ConsumptionListQueryDto
{
    public Guid? JobId { get; init; }
    public Guid? ItemId { get; init; }
    public Guid? LotId { get; init; }
    public string? LotNumber { get; init; }
    public DateTimeOffset? From { get; init; }
    public DateTimeOffset? To { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}