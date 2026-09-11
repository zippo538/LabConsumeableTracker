using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.UserDTOs;

namespace LabConsumableExpiryTracker.Application.Services.Interfaces;

public interface IUserService
{
    Task<ServiceResult<UserResponseDto>> CreateUserAsync(CreateUserRequestDto request);
    Task<ServiceResult<UserResponseDto>> GetUserProfileAsync(Guid userId);
    Task<ServiceResult<List<UserResponseDto>>> GetAllUsersAsync();
    Task<ServiceResult<UserResponseDto>> UpdateUserAsync(Guid id, UpdateUserRequestDto request);
    Task<ServiceResult<bool>> SetUserActiveStatusAsync(Guid currentAdminId, Guid targetUserId, bool isActive);

}