using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.ScientistDTOs;

namespace LabConsumableExpiryTracker.Application.Services.Interfaces;

public interface IScientistService
{
    Task<ServiceResult<ScientistResponseDto>> RegisterScientistAsync(CreateScientistRequestDto createScientistRequest);
    Task<ServiceResult<List<UserProfileResponseDto>>> GetAllScientistsAsync();
    Task<ServiceResult<UserProfileResponseDto>> GetUserProfileAsync(Guid userId, CancellationToken ct);
    Task<ServiceResult<ScientistResponseDto>> UpdateScientistAsync(Guid id, UpdateScientistRequestDto requestDto);
    Task<ServiceResult<bool>> DeleteScientistAsync(Guid id);
    Task<ServiceResult<bool>> SetScientistActiveStatusAsync(Guid id, bool inactive);
}