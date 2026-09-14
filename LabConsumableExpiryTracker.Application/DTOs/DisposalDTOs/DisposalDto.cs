namespace LabConsumableExpiryTracker.Application.DTOs.DisposalDTOs;

public class DisposalDto
{
    public Guid Id { get; set; }
    public Guid LotId { get; set; }
    public decimal Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTimeOffset DisposedAt { get; set; }
}
