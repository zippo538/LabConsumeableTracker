using LabConsumableExpiryTracker.Application.DTOs.DashboardDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConsumableExpiryTracker.API.Controllers;

[ApiController]
[Route("api/dashboard/alerts")]
[Authorize]
public class DashboardAlertsController : ControllerBase
{
    private readonly IDashboardAlertService _service;

    public DashboardAlertsController(IDashboardAlertService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAlerts(
        [FromQuery] DashboardAlertQuery query,
        CancellationToken ct = default)
    {
        var response = await _service.GetAlertsAsync(query, ct);
        return response.Success ? Ok(response) : BadRequest(response);
    }
}
