using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConsumableExpiryTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LotController : ControllerBase
{
    private readonly ILotService _lotService;

    public LotController(ILotService lotService)
    {
        _lotService = lotService;
    }

    [HttpGet]
    [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
    public async Task<ActionResult<ServiceResult<IEnumerable<LotDto>>>> GetAll(CancellationToken ct)
    {
        var response = await _lotService.GetAllLot(ct);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
    public async Task<ActionResult<ServiceResult<LotDto>>> GetById(Guid id, CancellationToken ct)
    {
        var response = await _lotService.GetByIdLot(id, ct);
        return response.Success ? Ok(response) : NotFound(response);
    }



    [HttpPost]
    [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
    public async Task<ActionResult<ServiceResult<LotDto>>> Create([FromBody] CreateLotDto request, CancellationToken ct)
    {
        var response = await _lotService.CreateLot(request, ct);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return response.Success
        ? CreatedAtAction(nameof(GetById), new { id = response.Data!.Id }, response)
        : Conflict(response);

    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
    public async Task<ActionResult<ServiceResult<LotDto>>> Update(Guid id, [FromBody] UpdateLotDto request, CancellationToken ct)
    {
        var response = await _lotService.UpdateLot(id, request, ct);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
    public async Task<ActionResult<ServiceResult<bool>>> Delete(Guid id, CancellationToken ct)
    {
        var response = await _lotService.DeleteLot(id, ct);
        if (!response.Success)
        {
            return NotFound(response);
        }

        return response.Success ? Ok(response) : NotFound(response);
    }
    [HttpGet("summary")]
    [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
    public async Task<ActionResult<ServiceResult<IEnumerable<LotSummaryDto>>>> GetAllSummary(
    CancellationToken ct = default)
    {
        var response = await _lotService.GetAllSummary(ct);
        return response.Success ? Ok(response) : BadRequest(response);
    }
    [HttpGet("paged")]
    [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
    public async Task<ActionResult<ServiceResult<PagedResult<LotDto>>>> GetAllPage(
    [FromQuery] int page = 1,
    [FromQuery] int PageSize = 20,
    CancellationToken ct = default)
    {
        var response = await _lotService.GetAllLotPage(page, PageSize, ct);
        return response.Success
            ? Ok(response)
            : BadRequest(response);
    }
}

