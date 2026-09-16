namespace LabConsumableExpiryTracker.Application.DTOs.DisposalDTOs;

public class CreateDisposalDto
{
    public string LotId { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
}
