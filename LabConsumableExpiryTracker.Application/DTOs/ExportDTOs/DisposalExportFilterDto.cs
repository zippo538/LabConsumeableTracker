namespace LabConsumableExpiryTracker.Application.DTOs.ExportDTOs;

public class DisposalExportFilterDto
{
    public string Format { get; set; } = "csv";
    public Guid? ItemId { get; set; }
    public string? ItemName { get; set; } = default!;
    public Guid? LotId { get; set; }
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
}

