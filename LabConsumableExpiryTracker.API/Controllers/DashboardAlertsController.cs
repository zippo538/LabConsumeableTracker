using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.DashboardDTOs;
using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConsumableExpiryTracker.API.Controllers;

[ApiController]
[Route("api/dashboard/alerts")]
[Authorize(Roles = AllowedRoles)]
public class DashboardAlertsController(IDashboardAlertService service) : ControllerBase
{
    private const string AllowedRoles = nameof(UserRole.Scientist) + "," + nameof(UserRole.WarehouseAdmin);

    [HttpGet]
    public async Task<ActionResult<ServiceResult<PagedResult<DashboardAlertDto>>>> GetAlerts(
        [FromQuery] DashboardAlertQuery query,
        CancellationToken ct = default)
    {
        var response = await service.GetAlertsAsync(query, ct);
        return response.Success ? Ok(response) : BadRequest(response);
    }
}
