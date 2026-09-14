using AutoMapper;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using LabConsumableExpiryTracker.Application.DTOs.UserDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LabConsumableExpiryTracker.Application.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IRefreshTokenRepository refreshTokenRepository,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _refreshTokenRepository = refreshTokenRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ServiceResult<UserResponseDto>> CreateUserAsync(CreateUserRequestDto request)
    {
        var roleName = request.Role.ToString();
        
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            return ServiceResult<UserResponseDto>.ErrorResult($"Role '{roleName}' does not exist.");
        }
        
        var user = _mapper.Map<ApplicationUser>(request);

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning("Failed to create user {Username}.", request.Username);
            return ServiceResult<UserResponseDto>.ErrorResult("Failed to create user.", errors);
        }
        
        var roleResult = await _userManager.AddToRoleAsync(user, roleName);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user); 
            var errors = roleResult.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning("Failed to assign role {Role} to {Username}.", roleName, user.UserName);
            return ServiceResult<UserResponseDto>.ErrorResult("Failed to assign user role.", errors);
        }

        _logger.LogInformation("User {Username} created successfully with role {Role}.", user.UserName, roleName);

        var response = _mapper.Map<UserResponseDto>(user);
        response.Role = roleName;

        return ServiceResult<UserResponseDto>.SuccessResult(response, "User created successfully.");
    }

    public async Task<ServiceResult<UserResponseDto>> GetUserProfileAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null || !user.IsActive)
        {
            _logger.LogWarning("User {UserId} not found or inactive.", userId);
            return ServiceResult<UserResponseDto>.ErrorResult("User not found or inactive.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? string.Empty;

        var response = _mapper.Map<UserResponseDto>(user);
        response.Role = role;

        return ServiceResult<UserResponseDto>.SuccessResult(response, "User profile retrieved successfully.");
    }

    public async Task<ServiceResult<List<UserResponseDto>>> GetAllUsersAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        var resultList = new List<UserResponseDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var dto = _mapper.Map<UserResponseDto>(user);
            dto.Role = roles.FirstOrDefault() ?? string.Empty;
            resultList.Add(dto);
        }

        return ServiceResult<List<UserResponseDto>>.SuccessResult(resultList);
    }

    public async Task<ServiceResult<UserResponseDto>> UpdateUserAsync(Guid id, UpdateUserRequestDto request)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return ServiceResult<UserResponseDto>.ErrorResult("User not found.");
        }

        _mapper.Map(request, user);

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ServiceResult<UserResponseDto>.ErrorResult("Failed to update user.", errors);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var response = _mapper.Map<UserResponseDto>(user);
        response.Role = roles.FirstOrDefault() ?? string.Empty;

        return ServiceResult<UserResponseDto>.SuccessResult(response, "User updated successfully.");
    }
    public async Task<ServiceResult<bool>> SetUserActiveStatusAsync(Guid currentAdminId, Guid targetUserId, bool isActive)
    {
        if (currentAdminId == targetUserId && !isActive)
        {
            return ServiceResult<bool>.ErrorResult("You cannot deactivate your own account.");
        }

        var user = await _userManager.FindByIdAsync(targetUserId.ToString());
        if (user is null)
        {
            return ServiceResult<bool>.ErrorResult("User not found.");
        }

        user.IsActive = isActive;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ServiceResult<bool>.ErrorResult("Failed to update user status.", errors);
        }
        
        if (!isActive)
        {
            await _refreshTokenRepository.RevokeAllByUserIdAsync(targetUserId);
        }

        _logger.LogInformation("User {Username} status changed to {IsActive}.", user.UserName, isActive);
        return ServiceResult<bool>.SuccessResult(true, $"User {(isActive ? "activated" : "deactivated")} successfully.");
    }
}