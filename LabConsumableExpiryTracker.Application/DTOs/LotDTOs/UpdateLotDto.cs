namespace LabConsumableExpiryTracker.Application.DTOs.LotDTOs
{
    public class UpdateLotDto
    {
        public string LotNumber { get; set; } = string.Empty;
        public string? SupplierLotNumber { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public DateOnly ExpiryDate { get; set; }
        public string StorageLocation { get; set; } = string.Empty;
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}