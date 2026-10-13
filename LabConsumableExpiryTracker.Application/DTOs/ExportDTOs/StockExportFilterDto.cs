namespace LabConsumableExpiryTracker.Application.DTOs.ExportDTOs;

public class StockExportFilterDto
{
    public string Format { get; init; } = "csv";
    public Guid? ItemId { get; init; }
    public bool? IsLowStock { get; init; }
}