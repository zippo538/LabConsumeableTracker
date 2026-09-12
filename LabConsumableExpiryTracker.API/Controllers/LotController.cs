using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConsumableExpiryTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize]
public class LotController : ControllerBase
{
    private readonly ILotService _lotService;

    public LotController(ILotService lotService)
    {
        _lotService = lotService;
    }

    [HttpGet]
    // [Authorize(Roles = "WarehouseAdmin")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var response = await _lotService.GetAllLot(ct);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpGet("{id:guid}")]
    // [Authorize(Roles = "WarehouseAdmin")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var response = await _lotService.GetByIdLot(id, ct);
        return response.Success ? Ok(response) : NotFound(response);
    }



    [HttpPost]
    // [Authorize(Roles = "WarehouseAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateLotDto request, CancellationToken ct)
    {
        var response = await _lotService.CreateLot(request, ct);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
    }

    [HttpPut("{id:guid}")]
    // [Authorize(Roles = "WarehouseAdmin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLotDto request, CancellationToken ct)
    {
        var response = await _lotService.UpdateLot(id, request, ct);
        return response.Success ? NoContent() : NotFound(response);
    }

    [HttpDelete("{id:guid}")]
    // [Authorize(Roles = "WarehouseAdmin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var response = await _lotService.DeleteLot(id, ct);
        return response.Success ? NoContent() : NotFound(response);
    }
    [HttpGet("summary")]
    // [Authorize(Roles = "WarehouseAdmin")]
    public async Task<ActionResult<ServiceResult<IEnumerable<LotSummaryDto>>>> GetAllSummary(
    CancellationToken ct = default)
    {
        var response = await _lotService.GetAllSummary(ct);
        return Ok(response);
    }
}

