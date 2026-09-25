namespace LabConsumableExpiryTracker.Application.DTOs.ExportDTOs;

public class DisposalExportFilterDto
{
    public string Format { get; init; } = "csv";
    public Guid? ItemId { get; init; }
    public string? ItemName { get; set; } = default!;
    public Guid? LotId { get; init; }
    public DateTimeOffset? From { get; init; }
    public DateTimeOffset? To { get; init; }
}

