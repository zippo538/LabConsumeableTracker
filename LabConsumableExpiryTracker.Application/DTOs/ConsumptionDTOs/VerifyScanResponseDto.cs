using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.DTOs.ConsumptionDTOs;

public class VerifyScanResponseDto
{
    public Guid ConsumptionId { get; set; }
    public Guid LotId { get; set; }
    public string LotNumber { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public ConsumptionStatus Status { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
    public UpdatedLotDto UpdatedLot { get; set; } = new();
}