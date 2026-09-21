using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.DisposalDTOs;
using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConsumableExpiryTracker.API.Controllers;

[ApiController]
[Route("api/disposals")]
[Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
public class DisposalController : ControllerBase
{
    public const string AllowedRoles = nameof(UserRole.Scientist) + "," + nameof(UserRole.WarehouseAdmin);
    private readonly IAuditService _auditService;

    public DisposalController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<ActionResult<ServiceResult<PagedResult<DisposalAuditDto>>>> GetDisposals(
        [FromQuery] AuditQuery query,
        CancellationToken ct)
    {
        var response = await _auditService.GetDisposalsAsync(query, ct);

        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServiceResult<DisposalAuditDto>>> GetDisposalbyId(
        Guid id,
        CancellationToken ct)
    {
        var response = await _auditService.GetDisposalByIdAsync(id, ct);

        return response.Success ? Ok(response) : NotFound(response);
    }
}
