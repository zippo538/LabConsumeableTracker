using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.ItemDTOs;
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
public class ItemController(
    IItemService itemService,
    ILotService lotService
) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<ServiceResult<IEnumerable<ItemDto>>>> GetAll
    (
        CancellationToken ct = default)
    {
        var response = await itemService.GetAllItem(ct);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<ServiceResult<ItemDto>>> GetById(
        Guid id,
        CancellationToken ct = default)
    {
        var response = await itemService.GetByIdItem(id, ct);

        return response.Success
            ? Ok(response)
            : NotFound(response);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResult<ItemDto>>> Create(
        [FromForm] CreateItemDto dto,
        CancellationToken ct)
    {
        var result = await itemService.CreateItem(dto, ct);
        return result.Success? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result)
            : Conflict(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
    public async Task<ActionResult<ServiceResult<ItemDto>>> Update(
        Guid id,
        [FromForm] UpdateItemDto dto,
        CancellationToken ct = default)
    {
        var response = await itemService.UpdateItem(id, dto, ct);

        return response.Success ? Ok(response) : Conflict(response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken ct)
    {
        var result = await itemService.DeleteItem(id, ct);

        if (result.Success) return Ok(result);
        if (result.Message == "Item not found.")
            return NotFound(result);

        return Conflict(result);

    }

    [HttpGet("item/{itemId:guid}/summary")]
    [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
    public async Task<ActionResult<ServiceResult<IEnumerable<LotSummaryDto>>>> GetSummaryByItemId(
        Guid itemId,
        CancellationToken ct = default)
    {
        var response = await lotService.GetSummaryByItemId(itemId, ct);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpGet("paged")]
    [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
    public async Task<ActionResult<ServiceResult<PagedResult<ItemDto>>>> GetAllPage(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var response = await itemService.GetAllItemPage(page, pageSize, ct);
        return response.Success
            ? Ok(response)
            : BadRequest(response);
    }
}