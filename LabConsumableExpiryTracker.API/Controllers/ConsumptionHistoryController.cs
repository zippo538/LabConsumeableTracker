using System.Security.Claims;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.ConsumptionHistoryDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConsumableExpiryTracker.API.Controllers;

[ApiController]
[Route("api/consumptions-history")]
public class ConsumptionsHistoryController(IConsumptionHistoryService consumptionHistoryService) : ControllerBase
{
    [HttpGet("me")]
    [Authorize(Roles = nameof(UserRole.Scientist))]
    public async Task<ActionResult<ServiceResult<MyConsumptionsResponseDto>>> GetMy(
        [FromQuery] ConsumptionListQueryDto query,
        CancellationToken ct)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var consumedBy))
        {
            return Unauthorized(ServiceResult<MyConsumptionsResponseDto>.ErrorResult("Invalid user token."));
        }

        var response = await consumptionHistoryService.GetMyConsumptions(consumedBy, query, ct);

        return response.Success
            ? Ok(response)
            : BadRequest(response);
    }
        
    [HttpGet("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.Scientist))]
    public async Task<ActionResult<ServiceResult<ConsumptionHistoryDetailDto>>> GetById(
        Guid id,
        CancellationToken ct)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdClaim) ||
            !Guid.TryParse(userIdClaim, out var consumedBy))
        {
            return Unauthorized(
                ServiceResult<ConsumptionHistoryDetailDto>.ErrorResult(
                    "Invalid user token."));
        }

        var response = await consumptionHistoryService.GetConsumptionDetail(
            id,
            consumedBy,
            ct);

        return response.Success
            ? Ok(response)
            : NotFound(response);
    }
}