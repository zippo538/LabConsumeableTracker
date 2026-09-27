namespace LabConsumableExpiryTracker.Application.DTOs.DisposalDTOs;

public class CreateDisposalDto
{
    public decimal Quantity { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string ExpectedRowVersion { get; init; } = string.Empty;
}
