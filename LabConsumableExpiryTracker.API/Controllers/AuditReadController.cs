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
public class AuditReadController(IAuditService auditService) : ControllerBase
{
    public const string AllowedRoles = nameof(UserRole.Scientist) + "," + nameof(UserRole.WarehouseAdmin);

    [HttpGet("audit-events")]
    public async Task<ActionResult<ServiceResult<PagedResult<AuditEventDto>>>> GetAuditEvents(
       [FromQuery] AuditQuery query,
       CancellationToken ct)
    {
        query = query with { Search = NormalizeSearch(query.Search) };
        var response = await auditService.GetAuditEventsAsync(query, ct);

        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpGet("consumptions")]
    public async Task<ActionResult<ServiceResult<PagedResult<ConsumptionAuditDto>>>> GetConsumptions(
        [FromQuery] AuditQuery query,
        CancellationToken ct)
    {
        query = query with { Search = NormalizeSearch(query.Search) };
        var response = await auditService.GetConsumptionsAsync(query, ct);

        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpGet("consumptions/{id:guid}")]
    public async Task<ActionResult<ServiceResult<ConsumptionAuditDto>>> GetConsumption(
        Guid id,
        CancellationToken ct)
    {
        
        var response = await auditService.GetConsumptionByIdAsync(id, ct);

        return response.Success ? Ok(response) : NotFound(response);
    }

    private static string? NormalizeSearch(string? search) =>
        string.IsNullOrWhiteSpace(search) ? null : search.Trim();
}
