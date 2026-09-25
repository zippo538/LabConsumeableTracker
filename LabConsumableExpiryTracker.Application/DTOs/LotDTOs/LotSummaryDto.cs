namespace LabConsumableExpiryTracker.Application.DTOs.LotDTOs;

public class LotSummaryDto
{
    public Guid LotId { get; init; }
    public required string LotNumber { get; init; }
    public decimal RemainingQuantity { get; init; }
    public DateOnly ExpiryDate { get; init; }
    public required string Status { get; init; }
        
}