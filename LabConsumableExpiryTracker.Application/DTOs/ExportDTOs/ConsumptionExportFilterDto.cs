namespace LabConsumableExpiryTracker.Application.DTOs.ExportDTOs;

public class ConsumptionExportFilterDto
{
    public string Format { get; set; } = "csv";
    public Guid? JobId { get; set; }
    public Guid? ItemId { get; set; }
    public Guid? LotId { get; set; }
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
}
