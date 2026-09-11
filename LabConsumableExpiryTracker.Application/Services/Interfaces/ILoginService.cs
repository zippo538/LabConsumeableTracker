using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.Login;

namespace LabConsumableExpiryTracker.Application.Services.Interfaces;

public interface ILoginService
{
    Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginRequestDto request);
}