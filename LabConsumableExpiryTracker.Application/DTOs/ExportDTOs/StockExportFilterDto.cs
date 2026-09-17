namespace LabConsumableExpiryTracker.Application.DTOs.ExportDTOs;

public class StockExportFilterDto
{
    public required string Format { get; set; } // wajib "csv" untuk sekarang
    public Guid? ItemId { get; set; }
    public bool? IsLowStock { get; set; }
}