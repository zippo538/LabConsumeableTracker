using FluentValidation;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.ItemDTOs;
using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConsumableExpiryTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize]
    public class ItemController : ControllerBase
    {
        private readonly IItemService _itemService;
        private readonly ILotService _lotService;
        private readonly IValidator<CreateItemDto> _createItemValidator;
        private readonly IValidator<UpdateItemDto> _updateItemValidator;


        public ItemController(
            IItemService itemService,
            IValidator<CreateItemDto> createItemValidator,
            IValidator<UpdateItemDto> updateItemValidator,
            ILotService lotService
            )
        {
            _itemService = itemService;
            _createItemValidator = createItemValidator;
            _updateItemValidator = updateItemValidator;
            _lotService = lotService;
        }

        [HttpGet]
        // [Authorize(Roles = "WarehouseAdmin")]
        public async Task<ActionResult<ServiceResult<IEnumerable<ItemDto>>>> GetAll(
            CancellationToken ct = default)
        {
            var response = await _itemService.GetAllItem(ct);
            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        // [Authorize(Roles = "WarehouseAdmin")]
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
        // [Authorize(Roles = "WarehouseAdmin")]
        public async Task<ActionResult<ServiceResult<ItemDto>>> Create(
            [FromBody] CreateItemDto dto,
            CancellationToken ct = default)
        {
            var validation = await _createItemValidator.ValidateAsync(dto, ct);

            if (!validation.IsValid)
            {
                return BadRequest(ServiceResult<ItemDto>.ErrorResult(
                    "Validation failed.",
                    validation.Errors
                        .Select(error => error.ErrorMessage)
                        .ToList()));
            }

            var response = await _itemService.CreateItem(dto, ct);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = response.Data!.Id },
                response);
        }

        [HttpPut("{id:guid}")]
        // [Authorize(Roles = "WarehouseAdmin")]
        public async Task<ActionResult<ServiceResult<ItemDto>>> Update(
            Guid id,
            [FromBody] UpdateItemDto dto,
            CancellationToken ct = default)
        {
            var validation = await _updateItemValidator.ValidateAsync(dto, ct);

            if (!validation.IsValid)
            {
                return BadRequest(ServiceResult<ItemDto>.ErrorResult(
                    "Validation failed.",
                    validation.Errors
                        .Select(error => error.ErrorMessage)
                        .ToList()));
            }

            var response = await _itemService.UpdateItem(id, dto, ct);

            return response.Success
                ? Ok(response)
                : NotFound(response);
        }

        [HttpDelete("{id:guid}")]
        // [Authorize(Roles = "WarehouseAdmin")]
        public async Task<ActionResult<ServiceResult<bool>>> Delete(
            Guid id,
            CancellationToken ct = default)
        {
            var response = await _itemService.DeleteItem(id, ct);

            return response.Success
                ? Ok(response)
                : NotFound(response);
        }

        //lot summary
        [HttpGet("item/{itemId:guid}/summary")]
        // [Authorize(Roles = "WarehouseAdmin")]
        public async Task<ActionResult<ServiceResult<IEnumerable<LotSummaryDto>>>> GetSummaryByItemId(
            Guid itemId,
            CancellationToken ct = default)
        {
            var response = await _lotService.GetSummaryByItemId(itemId, ct);
            return Ok(response);
        }
    }
}
