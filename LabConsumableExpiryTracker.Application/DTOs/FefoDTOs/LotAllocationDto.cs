namespace LabConsumableExpiryTracker.Application.DTOs.FefoDTOs;

public class LotAllocationDto
{
    public Guid LotId { get; set; }
    public string LotNumber { get; set; } = default!;
    public DateOnly ExpiryDate { get; set; }
    public decimal RemainingQuantity { get; set; }
    public decimal Quantity { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}