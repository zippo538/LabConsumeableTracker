namespace LabConsumableExpiryTracker.Application.DTOs.DisposalDTOs;

public class DisposalDto
{
    public Guid Id { get; init; }
    public Guid LotId { get; init; }
    public decimal Quantity { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTimeOffset DisposedAt { get; init; }
}
