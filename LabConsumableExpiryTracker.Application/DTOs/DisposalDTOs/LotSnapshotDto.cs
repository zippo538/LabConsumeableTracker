namespace LabConsumableExpiryTracker.Application.DTOs.DisposalDTOs;

public class LotSnapshotDto
{
    public string Id { get; init; } = string.Empty;
    public decimal RemainingQuantity { get; init; }
    public string AdministrativeStatus { get; init; } = string.Empty;
    public string RowVersion { get; init; } = string.Empty; 
    public string? EvaluatedAt { get; init; }
}