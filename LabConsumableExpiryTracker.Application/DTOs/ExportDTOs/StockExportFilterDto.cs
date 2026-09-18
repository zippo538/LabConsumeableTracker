namespace LabConsumableExpiryTracker.Application.DTOs.ExportDTOs;

public class StockExportFilterDto
{
    public string Format { get; set; } = "csv";
    public Guid? ItemId { get; set; }
    public bool? IsLowStock { get; set; }
}