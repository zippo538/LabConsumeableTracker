using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.DTOs.ConsumptionHistoryDTOs;

public class ConsumptionListItemDto
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string? JobNumber { get; set; } = default!;
    public Guid LotId { get; set; }
    public string LotNumber { get; init; } = default!;
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = default!;
    public string ItemCode { get; set; } = default!;
    public decimal Quantity { get; init; }
    public UnitOfMeasure BaseUnit { get; set; }
    public DateTimeOffset ConsumedAt { get; set; }
    public Guid ConsumedBy { get; set; }
}