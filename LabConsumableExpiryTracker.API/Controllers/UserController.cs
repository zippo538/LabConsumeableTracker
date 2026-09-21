using System.Security.Claims;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.UserDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConsumableExpiryTracker.API.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    // ✅ PERUBAHAN: Route "profile" diubah menjadi "me" sesuai SRS 7.2 & Frontend
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ServiceResult<UserResponseDto>>> GetMe(CancellationToken ct)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ServiceResult<UserResponseDto>.ErrorResult("Invalid user token."));
        }

        var result = await _userService.GetUserProfileAsync(userId);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    // ✅ TAMBAHAN: Endpoint untuk update FullName sendiri (SRS FR-11-01)
    [Authorize]
    [HttpPatch("me")]
    public async Task<ActionResult<ServiceResult<UserResponseDto>>> UpdateMe(
        [FromBody] UpdateFullNameRequestDto request,
        CancellationToken ct)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ServiceResult<UserResponseDto>.ErrorResult("Invalid user token."));
        }

        // Anda perlu membuat DTO sederhana: public class UpdateFullNameRequestDto { public string FullName { get; set; } }
        // Atau gunakan logic mapping di service untuk update partial
        var result = await _userService.UpdateUserAsync(userId, new UpdateUserRequestDto { FullName = request.FullName });
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
    [HttpPost]
    public async Task<ActionResult<ServiceResult<UserResponseDto>>> CreateUser(
        [FromBody] CreateUserRequestDto request)
    {
        var result = await _userService.CreateUserAsync(request);
        if (!result.Success)
            return BadRequest(result);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
    [HttpGet]
    public async Task<ActionResult<ServiceResult<List<UserResponseDto>>>> GetAllUsers()
    {
        var result = await _userService.GetAllUsersAsync();
        return Ok(result);
    }

    [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ServiceResult<UserResponseDto>>> UpdateUser(
        Guid id,
        [FromBody] UpdateUserRequestDto request)
    {
        var result = await _userService.UpdateUserAsync(id, request);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
    [HttpPut("{id:guid}/activate")]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        var currentAdminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _userService.SetUserActiveStatusAsync(currentAdminId, id, true);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
    [HttpPut("{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        var currentAdminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _userService.SetUserActiveStatusAsync(currentAdminId, id, false);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
}