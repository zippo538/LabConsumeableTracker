using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConsumableExpiryTracker.API.Controllers;

[ApiController]
[Route("api/disposals")]
[Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
public class DisposalController(IAuditService auditService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ServiceResult<PagedResult<DisposalAuditDto>>>> GetDisposals(
        [FromQuery] AuditQuery query,
        CancellationToken ct)
    {
        query = query with { Search = NormalizeSearch(query.Search) };
        var response = await auditService.GetDisposalsAsync(query, ct);

        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServiceResult<DisposalAuditDto>>> GetDisposalbyId(
        Guid id,
        CancellationToken ct)
    {
        var response = await auditService.GetDisposalByIdAsync(id, ct);

        return response.Success ? Ok(response) : NotFound(response);
    }
    private static string? NormalizeSearch(string? search) =>
        string.IsNullOrWhiteSpace(search) ? null : search.Trim();
}
