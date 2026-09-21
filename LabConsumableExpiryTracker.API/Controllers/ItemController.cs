using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.ItemDTOs;
using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConsumableExpiryTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ItemController : ControllerBase
    {
        private readonly IItemService _itemService;
        private readonly ILotService _lotService;


        public ItemController(
            IItemService itemService,
            ILotService lotService
        )
        {
            _itemService = itemService;
            _lotService = lotService;
        }

        [HttpGet]
        [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
        public async Task<ActionResult<ServiceResult<IEnumerable<ItemDto>>>> GetAll
        (
            CancellationToken ct = default)
        {
            var response = await _itemService.GetAllItem(ct);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
        public async Task<ActionResult<ServiceResult<ItemDto>>> GetById(
            Guid id,
            CancellationToken ct = default)
        {
            var response = await _itemService.GetByIdItem(id, ct);

            return response.Success
                ? Ok(response)
                : NotFound(response);
        }

        [HttpPost]
        public async Task<ActionResult<ServiceResult<ItemDto>>> Create(
            [FromForm] CreateItemDto dto,
            CancellationToken ct)
        {
            var result = await _itemService.CreateItem(dto, ct);

            if (result.Data?.ImagePath is not null)
            {
                result.Data.ImagePath = $"{Request.Scheme}://{Request.Host}/{result.Data.ImagePath}";
            }

            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
        public async Task<ActionResult<ServiceResult<ItemDto>>> Update(
            Guid id,
            [FromForm] UpdateItemDto dto,
            CancellationToken ct = default)
        {
            var response = await _itemService.UpdateItem(id, dto, ct);

            return response.Success ? Ok(response) : Conflict(response);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(
            Guid id,
            CancellationToken ct)
        {
            var result = await _itemService.DeleteItem(id, ct);

            if (!result.Success)
            {
                if (result.Message == "Item not found.")
                    return NotFound(result);

                return Conflict(result);
            }

            return Ok(result);
        }

        [HttpGet("item/{itemId:guid}/summary")]
        [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
        public async Task<ActionResult<ServiceResult<IEnumerable<LotSummaryDto>>>> GetSummaryByItemId(
            Guid itemId,
            CancellationToken ct = default)
        {
            var response = await _lotService.GetSummaryByItemId(itemId, ct);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet("paged")]
        [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
        public async Task<ActionResult<ServiceResult<PagedResult<ItemDto>>>> GetAllPage(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var response = await _itemService.GetAllItemPage(page, pageSize, ct);
            return response.Success
                ? Ok(response)
                : BadRequest(response);
        }
    }
}