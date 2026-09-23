using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.DTOs.ConsumptionHistoryDTOs;

public class ConsumptionHistoryDetailDto
{
    public Guid Id { get; init; }

    public Guid JobId { get; init; }
    public string JobNumber { get; init; } = string.Empty;
    public string JobDescription { get; init; } = string.Empty;

    public Guid ItemId { get; init; }
    public string ItemCode { get; init; } = string.Empty;
    public string ItemName { get; init; } = string.Empty;
    public UnitOfMeasure BaseUnit { get; init; } 

    public Guid LotId { get; init; }
    public string LotNumber { get; init; } = string.Empty;
    public DateOnly ExpiryDate { get; init; }
    public string StorageLocation { get; init; } = string.Empty;

    public decimal Quantity { get; init; }

    public Guid ConsumedBy { get; init; }
    public DateTimeOffset ConsumedAt { get; init; }
}