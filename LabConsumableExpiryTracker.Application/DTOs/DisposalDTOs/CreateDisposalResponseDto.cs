namespace LabConsumableExpiryTracker.Application.DTOs.DisposalDTOs;

public class CreateDisposalResponseDto
{
    public DisposalDto Disposal { get; init; } = null!;
    public LotSnapshotDto Lot { get; set; } = null!;
}