using LabConsumableExpiryTracker.Application.DTOs.DisposalDTOs;
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
    private readonly IDisposalService _disposalService;

    public DisposalController(IDisposalService disposalService)
    {
        _disposalService = disposalService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateDisposalDto request,
        CancellationToken ct = default)
    {
        var response = await _disposalService.CreateDisposalAsync(request, ct);

        if (response.Success)
            return StatusCode(StatusCodes.Status201Created, response);

        return response.Message == "Lot not found."
            ? NotFound(response)
            : BadRequest(response);
    }
}
