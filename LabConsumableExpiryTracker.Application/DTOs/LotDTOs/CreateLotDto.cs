
namespace LabConsumableExpiryTracker.Application.DTOs.LotDTOs
{
    public class CreateLotDto
    {
        public string ItemId { get; set; } = string.Empty;
        public string LotNumber { get; set; } = string.Empty;
        public string? SupplierLotNumber { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal InitialQuantity { get; set; }
        public DateTimeOffset ReceivedAt { get; set; }
        public DateOnly ExpiryDate { get; set; }
        public string StorageLocation { get; set; } = string.Empty;
    }
}