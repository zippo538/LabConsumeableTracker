using FluentValidation;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.ItemDTOs;
using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
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
        [Authorize]
        public async Task<ActionResult<ServiceResult<IEnumerable<ItemDto>>>> GetAll(
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
        [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
        public async Task<ActionResult<ServiceResult<ItemDto>>> Create(
            [FromBody] CreateItemDto dto,
            CancellationToken ct = default)
        {
          
            var response = await _itemService.CreateItem(dto, ct);

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
        public async Task<ActionResult<ServiceResult<ItemDto>>> Update(
            Guid id,
            [FromBody] UpdateItemDto dto,
            CancellationToken ct = default)
        {
           
            var response = await _itemService.UpdateItem(id, dto, ct);

            return response.Success ? Ok(response) : Conflict(response);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
        public async Task<ActionResult<ServiceResult<bool>>> Delete(
            Guid id,
            CancellationToken ct = default)
        {
            var response = await _itemService.DeleteItem(id, ct);

            return response.Success ? Ok(response) : NotFound(response);
        }

        //lot summary
        [HttpGet("item/{itemId:guid}/summary")]
        [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
        public async Task<ActionResult<ServiceResult<IEnumerable<LotSummaryDto>>>> GetSummaryByItemId(
            Guid itemId,
            CancellationToken ct = default)
        {
            var response = await _lotService.GetSummaryByItemId(itemId, ct);
            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}
