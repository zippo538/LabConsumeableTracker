using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.DisposalDTOs;
using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConsumableExpiryTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
public class LotController : ControllerBase
{
    private readonly ILotService _lotService;
    private readonly IDisposalService _disposalService;

    public LotController(ILotService lotService, IDisposalService disposalService)
    {
        _lotService = lotService;
        _disposalService = disposalService;
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
    [HttpPost("{id:guid}/disposals")]
    [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
    public async Task<ActionResult<ServiceResult<CreateDisposalResponseDto>>> DisposeLot(
    Guid id,
    [FromBody] CreateDisposalDto request,
    CancellationToken ct = default)
    {
        var response = await _disposalService.CreateDisposalAsync(id, request, ct);

        if (!response.Success)
        {
            return response.Message switch
            {
                "Lot not found." => NotFound(response),
                "Concurrency conflict. The lot was modified by another user." =>
                    StatusCode(StatusCodes.Status409Conflict, response),
                _ => BadRequest(response)
            };
        }
        return CreatedAtAction(
            nameof(GetById), 
            new { id = response.Data!.Disposal.Id },
            response
        );
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
    [FromQuery] int pageSize = 20,
    [FromQuery] string? search = null,
    [FromQuery] Guid? itemId = null,
    [FromQuery] string? administrativeStatus = null,
    [FromQuery] string? expiryCondition = null, 
    CancellationToken ct = default)
    {
        var response = await _lotService.GetAllLotPage(
            ct, page, pageSize, search, itemId, administrativeStatus, expiryCondition);
        return response.Success ? Ok(response) : BadRequest(response);
    }
}

