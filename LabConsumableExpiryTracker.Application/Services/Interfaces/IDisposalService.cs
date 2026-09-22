using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.DisposalDTOs;

namespace LabConsumableExpiryTracker.Application.Services.Interfaces;

public interface IDisposalService
{
    Task<ServiceResult<CreateDisposalResponseDto>> CreateDisposalAsync(
        Guid lotId,
        CreateDisposalDto request,
        CancellationToken ct = default);
}
