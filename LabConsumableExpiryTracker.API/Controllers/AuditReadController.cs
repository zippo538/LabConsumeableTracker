using LabConsumableExpiryTracker.Application.DTOs.AuditDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConsumableExpiryTracker.API.Controllers;

[ApiController]
[Route("api")]
[Authorize(Roles = "scientis,WarehouseAdmin")]
public sealed class AuditReadController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditReadController(IAuditService auditService) => _auditService = auditService;

    [HttpGet("audit-events")]
    public async Task<IActionResult> GetAuditEvents([FromQuery] AuditQuery query, CancellationToken ct) =>
        ToActionResult(await _auditService.GetAuditEventsAsync(query, ct));

    [HttpGet("consumptions")]
    public async Task<IActionResult> GetConsumptions([FromQuery] AuditQuery query, CancellationToken ct) =>
        ToActionResult(await _auditService.GetConsumptionsAsync(query, ct));

    [HttpGet("consumptions/{id:guid}")]
    public async Task<IActionResult> GetConsumption(Guid id, CancellationToken ct) =>
        ToActionResult(await _auditService.GetConsumptionByIdAsync(id, ct));

    [HttpGet("disposals")]
    public async Task<IActionResult> GetDisposals([FromQuery] AuditQuery query, CancellationToken ct) =>
        ToActionResult(await _auditService.GetDisposalsAsync(query, ct));

    [HttpGet("disposals/{id:guid}")]
    public async Task<IActionResult> GetDisposal(Guid id, CancellationToken ct) =>
        ToActionResult(await _auditService.GetDisposalByIdAsync(id, ct));

    private ActionResult ToActionResult<T>(LabConsumableExpiryTracker.Application.Common.Result.ServiceResult<T> response)
    {
        if (response.Success) return Ok(response);
        return response.Message.EndsWith("not found.", StringComparison.Ordinal)
            ? NotFound(response)
            : BadRequest(response);
    }
}
