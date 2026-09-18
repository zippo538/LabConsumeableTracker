using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.DTOs.ExportDTOs;

public class ConsumptionExportRowDto
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public Guid LotId { get; set; }
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = default!;
    public decimal Quantity { get; set; }
    public UnitOfMeasure BaseUnit { get; set; }
    public DateTimeOffset ConsumedAt { get; set; }
    public string  ConsumedBy { get; set; } = default!;
}