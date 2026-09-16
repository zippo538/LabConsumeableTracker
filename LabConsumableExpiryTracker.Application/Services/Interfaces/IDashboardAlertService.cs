using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.DashboardDTOs;
using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;

namespace LabConsumableExpiryTracker.Application.Services.Interfaces;

public interface IDashboardAlertService
{
    Task<ServiceResult<PagedResult<DashboardAlertDto>>> GetAlertsAsync(
        DashboardAlertQuery query,
        CancellationToken ct = default);
}
