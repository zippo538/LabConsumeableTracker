namespace LabConsumableExpiryTracker.Application.DTOs.ConsumptionDTOs;

public class VerifyScanRequestDto
{
    public Guid ItemId { get; init; }
    public string ScannedLotNumber { get; init; } = string.Empty;
}