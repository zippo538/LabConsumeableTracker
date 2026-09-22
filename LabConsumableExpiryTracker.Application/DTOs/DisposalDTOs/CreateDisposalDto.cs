namespace LabConsumableExpiryTracker.Application.DTOs.DisposalDTOs;

public class CreateDisposalDto
{
    public decimal Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string ExpectedRowVersion { get; set; } = string.Empty;
}
