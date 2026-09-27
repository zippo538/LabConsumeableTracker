using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.DTOs.ExportDTOs;

public class ConsumptionExportRowDto
{
    public Guid Id { get; init; }
    public Guid JobId { get; init; }
    public Guid LotId { get; init; }
    public Guid ItemId { get; init; }
    public string ItemName { get; init; } = default!;
    public decimal Quantity { get; init; }
    public UnitOfMeasure BaseUnit { get; init; }
    public DateTimeOffset ConsumedAt { get; init; }
    public string  ConsumedBy { get; init; } = default!;
}