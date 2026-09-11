using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.ScientistDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConsumableExpiryTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScientistController : ControllerBase
{
    private readonly IScientistService _scientistService;


    public ScientistController(IScientistService scientistService)
    {
        _scientistService = scientistService;
    }
  
    [Authorize(Roles = "WarehouseAdmin")]
    [HttpPost]
    public async Task<ActionResult<ServiceResult<ScientistResponseDto>>> Register(CreateScientistRequestDto createScientistRequest)
    {
        var response = await _scientistService.RegisterScientistAsync(createScientistRequest);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [Authorize(Roles = "WarehouseAdmin")]
    [HttpGet]
    public async Task<ActionResult<ServiceResult<List<ScientistResponseDto>>>> GetScientists()
    {
        var response = await _scientistService.GetAllScientistsAsync();

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [Authorize(Roles = "WarehouseAdmin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ServiceResult<ScientistResponseDto>>> UpdateScientist(
        Guid id,
        UpdateScientistRequestDto requestDto)
    {
        var response = await _scientistService.UpdateScientistAsync(id, requestDto);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [Authorize(Roles = "WarehouseAdmin")]
    [HttpPut("{id}/activate")]
    public async Task<IActionResult> ActivateScientist(Guid id)
    {
        var result = await _scientistService
            .SetScientistActiveStatusAsync(id, true);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "WarehouseAdmin")]
    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> DeactivateScientist(Guid id)
    {
        var result = await _scientistService
            .SetScientistActiveStatusAsync(id, false);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }


}