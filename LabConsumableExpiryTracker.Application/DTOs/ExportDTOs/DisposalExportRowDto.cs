using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.DTOs.ExportDTOs;

public class DisposalExportRowDto
{
    public Guid Id { get; init; }
    public Guid LotId { get; init; }
    public string LotNumber { get; init; } = string.Empty;
    public Guid ItemId { get; init; }
    public string ItemName { get; init; } = default!;
    public decimal Quantity { get; init; }
    public UnitOfMeasure BaseUnit { get; init; }
    public string Reason { get; init; } = default!;
    public DateTimeOffset DisposedAt { get; init; }
    public string DisposedBy { get; init; } = default!;
}