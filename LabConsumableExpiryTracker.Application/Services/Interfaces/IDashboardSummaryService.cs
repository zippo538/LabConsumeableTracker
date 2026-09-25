using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.DashboardDTOs;

namespace LabConsumableExpiryTracker.Application.Services.Interfaces;

public interface IDashboardSummaryService
{
    Task<ServiceResult<DashboardSummaryDto>> GetSummaryAsync(Guid? itemId, CancellationToken ct);
}