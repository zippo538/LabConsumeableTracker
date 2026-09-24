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
    public async Task<ActionResult<ServiceResult<PagedUsersResponseDto>>> GetAllUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? role = null,
        [FromQuery] string? status = null)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(currentUserId, out var currentUserGuid))
        {
            return Unauthorized();
        }

        var result = await _userService.GetAllUsersAsync(
            page,
            pageSize,
            search,
            role,
            status,
            currentUserGuid
        );

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
    
    [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
    [HttpGet("stats")]
    public async Task<ActionResult<ServiceResult<UserStatisticsResponseDto>>>
        GetUserStatistics()
    {
        var result = await _userService.GetUserStatisticsAsync();

        return Ok(result);
    }
    
    [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceResult<UserResponseDto>>> GetUserById(
        string id)
    {
        var result = await _userService.GetUserByIdAsync(id);

        return Ok(result);
    }
    
    [HttpPut("{id:guid}/password")]
    [Authorize(Roles = nameof(UserRole.WarehouseAdmin))]
    public async Task<ActionResult<ServiceResult<bool>>> ChangePassword(
        Guid id,
        [FromBody] ChangeUserPasswordRequestDto request)
    {
        var result = await _userService.ChangeUserPasswordAsync(
            id,
            request
        );

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}