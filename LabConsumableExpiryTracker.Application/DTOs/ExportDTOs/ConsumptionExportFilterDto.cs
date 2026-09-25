namespace LabConsumableExpiryTracker.Application.DTOs.ExportDTOs;

public class ConsumptionExportFilterDto
{
    public string Format { get; init; } = "csv";
    public Guid? JobId { get; init; }
    public Guid? ItemId { get; init; }
    public Guid? LotId { get; init; }
    public DateTimeOffset? From { get; init; }
    public DateTimeOffset? To { get; init; }
}
