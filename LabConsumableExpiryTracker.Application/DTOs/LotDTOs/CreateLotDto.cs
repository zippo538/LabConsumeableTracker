
namespace LabConsumableExpiryTracker.Application.DTOs.LotDTOs;

public class CreateLotDto
{
    public string ItemId { get; set; } = string.Empty;
    public string LotNumber { get; set; } = string.Empty;
    public string? SupplierLotNumber { get; init; }
    public string SupplierName { get; init; } = string.Empty;
    public decimal InitialQuantity { get; set; }
    public DateTimeOffset ReceivedAt { get; init; }
    public DateOnly ExpiryDate { get; init; }
    public string StorageLocation { get; init; } = string.Empty;
}