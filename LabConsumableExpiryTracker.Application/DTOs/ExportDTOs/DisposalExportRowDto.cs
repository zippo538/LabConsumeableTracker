using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.DTOs.ExportDTOs;

public class DisposalExportRowDto
{
    public Guid Id { get; set; }
    public Guid LotId { get; set; }
    public string LotNumber { get; set; }
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = default!;
    public decimal Quantity { get; set; }
    public UnitOfMeasure BaseUnit { get; set; }
    public string Reason { get; set; } = default!;
    public DateTimeOffset DisposedAt { get; set; }
    public string DisposedBy { get; set; } = default!;
}