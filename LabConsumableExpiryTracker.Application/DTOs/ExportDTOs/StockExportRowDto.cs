using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.DTOs.ExportDTOs;

public class StockExportRowDto
{
    public Guid ItemId { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public UnitOfMeasure BaseUnit { get; set; }
    public decimal UsableStock { get; set; }
    public decimal MinimumStock { get; set; }
    public bool IsLowStock { get; set; }
    public DateTimeOffset EvaluatedAt { get; set; }
}