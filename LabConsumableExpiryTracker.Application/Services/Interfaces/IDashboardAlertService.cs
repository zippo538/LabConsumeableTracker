using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.DashboardDTOs;

namespace LabConsumableExpiryTracker.Application.Services.Interfaces;

public interface IDashboardAlertService
{
    Task<ServiceResult<PagedResult<DashboardAlertDto>>> GetAlertsAsync(
        DashboardAlertQuery query,
        CancellationToken ct = default);
}
