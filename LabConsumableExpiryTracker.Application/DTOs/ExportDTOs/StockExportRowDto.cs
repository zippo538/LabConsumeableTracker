using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.DTOs.ExportDTOs;

public class StockExportRowDto
{
    public Guid ItemId { get; init; }
    public string Code { get; init; } = default!;
    public string Name { get; init; } = default!;
    public UnitOfMeasure BaseUnit { get; init; }
    public decimal UsableStock { get; init; }
    public decimal MinimumStock { get; init; }
    public bool IsLowStock { get; init; }
    public DateTimeOffset EvaluatedAt { get; init; }
}