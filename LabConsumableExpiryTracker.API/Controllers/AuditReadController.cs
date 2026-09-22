using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConsumableExpiryTracker.API.Controllers;

[ApiController]
[Route("api")]
[Authorize(Roles = AllowedRoles)]
public class AuditReadController : ControllerBase
{
    public const string AllowedRoles = nameof(UserRole.Scientist) + "," + nameof(UserRole.WarehouseAdmin);
    private readonly IAuditService _auditService;

    public AuditReadController(IAuditService auditService) => _auditService = auditService;

    [HttpGet("audit-events")]
    public async Task<ActionResult<ServiceResult<PagedResult<AuditEventDto>>>> GetAuditEvents(
       [FromQuery] AuditQuery query,
       CancellationToken ct)
    {
        var response = await _auditService.GetAuditEventsAsync(query, ct);

        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpGet("consumptions")]
    public async Task<ActionResult<ServiceResult<PagedResult<ConsumptionAuditDto>>>> GetConsumptions(
        [FromQuery] AuditQuery query,
        CancellationToken ct)
    {
        var response = await _auditService.GetConsumptionsAsync(query, ct);

        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpGet("consumptions/{id:guid}")]
    public async Task<ActionResult<ServiceResult<ConsumptionAuditDto>>> GetConsumption(
        Guid id,
        CancellationToken ct)
    {
        var response = await _auditService.GetConsumptionByIdAsync(id, ct);

        return response.Success ? Ok(response) : NotFound(response);
    }
}
