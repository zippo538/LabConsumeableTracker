namespace LabConsumableExpiryTracker.Application.DTOs.ConsumptionHistoryDTOs;

public class ConsumptionListQueryDto
{
    public Guid? JobId { get; set; }
    public Guid? ItemId { get; set; }
    public Guid? LotId { get; set; }
    public string? LotNumber { get; set; }
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}