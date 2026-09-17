namespace LabConsumableExpiryTracker.Application.DTOs.ExportDTOs;

public class DisposalExportFilterDto
{
    public required string Format { get; set; }
    public Guid? ItemId { get; set; }
    public string ItemName { get; set; } = default!;
    public Guid? LotId { get; set; }
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
}

