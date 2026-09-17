namespace LabConsumableExpiryTracker.Application.DTOs.ExportDTOs;

public class ConsumptionExportFilterDto
{
    public required string Format { get; set; }
    public Guid? JobId { get; set; }
    public Guid? ItemId { get; set; }
    public Guid? LotId { get; set; }
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
}
