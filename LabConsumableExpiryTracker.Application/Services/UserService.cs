using AutoMapper;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using LabConsumableExpiryTracker.Application.DTOs.UserDTOs;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
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
        response.Roles = new List<string> { roleName };

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
        var response = _mapper.Map<UserResponseDto>(user);
        response.Roles = roles.ToList();
        return ServiceResult<UserResponseDto>.SuccessResult(response, "User profile retrieved successfully.");
    }


public async Task<ServiceResult<PagedUsersResponseDto>> GetAllUsersAsync(
    int page,
    int pageSize,
    string? search,
    string? role,
    string? status,
    Guid currentUserId)
{
    page = page < 1 ? 1 : page;
    pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

    var query = _userManager.Users
        .AsNoTracking()
        .Where(u => u.Id != currentUserId)
        .AsQueryable();

    // filter search
    if (!string.IsNullOrWhiteSpace(search))
    {
        var keyword = search.Trim();

        query = query.Where(u =>
            (u.UserName != null &&
             u.UserName.Contains(keyword)) ||

            (u.Email != null &&
             u.Email.Contains(keyword)) ||

            (u.FullName != null &&
             u.FullName.Contains(keyword))
        );
    }

    // filter status
    if (!string.IsNullOrWhiteSpace(status))
    {
        if (status.Equals(
                "active",
                StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(u => u.IsActive);
        }
        else if (status.Equals(
                     "inactive",
                     StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(u => !u.IsActive);
        }
    }

    // filter role
    if (!string.IsNullOrWhiteSpace(role))
    {
        var usersInRole =
            await _userManager.GetUsersInRoleAsync(role);

        var userIds = usersInRole
            .Select(u => u.Id)
            .ToList();

        query = query.Where(u =>
            userIds.Contains(u.Id));
    }

    var totalCount = await query.CountAsync();

    var adminUsers =
        await _userManager.GetUsersInRoleAsync(
            nameof(UserRole.WarehouseAdmin));

    var adminIds = adminUsers
        .Select(u => u.Id)
        .ToHashSet();

    var users = await query
        .OrderByDescending(u => adminIds.Contains(u.Id))
        .ThenBy(u => u.UserName)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    var resultList = new List<UserResponseDto>();

    foreach (var user in users)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var dto = _mapper.Map<UserResponseDto>(user);
        dto.Roles = roles.ToList();

        resultList.Add(dto);
    }

    var response = new PagedUsersResponseDto
    {
        Items = resultList,
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize
    };

    return ServiceResult<PagedUsersResponseDto>
        .SuccessResult(response);
}
public async Task<ServiceResult<UserResponseDto>> UpdateUserAsync(
    Guid id,
    UpdateUserRequestDto request)
{
    var user = await _userManager.FindByIdAsync(id.ToString());

    if (user is null)
    {
        return ServiceResult<UserResponseDto>.ErrorResult(
            "User not found."
        );
    }

    user.FullName = request.FullName;

    var usernameResult = await _userManager.SetUserNameAsync(
        user,
        request.Username
    );

    if (!usernameResult.Succeeded)
    {
        var errors = usernameResult.Errors
            .Select(e => e.Description)
            .ToList();

        return ServiceResult<UserResponseDto>.ErrorResult(
            "Failed to update username.",
            errors
        );
    }

    var emailResult = await _userManager.SetEmailAsync(
        user,
        request.Email
    );

    if (!emailResult.Succeeded)
    {
        var errors = emailResult.Errors
            .Select(e => e.Description)
            .ToList();

        return ServiceResult<UserResponseDto>.ErrorResult(
            "Failed to update email.",
            errors
        );
    }

    var updateResult = await _userManager.UpdateAsync(user);

    if (!updateResult.Succeeded)
    {
        var errors = updateResult.Errors
            .Select(e => e.Description)
            .ToList();

        return ServiceResult<UserResponseDto>.ErrorResult(
            "Failed to update user.",
            errors
        );
    }

    var roles = await _userManager.GetRolesAsync(user);

    var response = _mapper.Map<UserResponseDto>(user);
    response.Roles = roles.ToList();

    return ServiceResult<UserResponseDto>.SuccessResult(
        response,
        "User updated successfully."
    );
}

    public async Task<ServiceResult<bool>> SetUserActiveStatusAsync(Guid currentAdminId, Guid targetUserId,
        bool isActive)
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
        return ServiceResult<bool>.SuccessResult(true,
            $"User {(isActive ? "activated" : "deactivated")} successfully.");
    }
    
    public async Task<ServiceResult<UserStatisticsResponseDto>> GetUserStatisticsAsync()
    {
        var totalCount = await _userManager.Users
            .CountAsync();

        var activeCount = await _userManager.Users
            .CountAsync(u => u.IsActive);

        var deactivatedCount = await _userManager.Users
            .CountAsync(u => !u.IsActive);

        var adminUsers = await _userManager.GetUsersInRoleAsync(
            nameof(UserRole.WarehouseAdmin)
        );

        var activeAdminCount = adminUsers
            .Count(u => u.IsActive);

        var response = new UserStatisticsResponseDto
        {
            TotalCount = totalCount,
            ActiveCount = activeCount,
            ActiveAdminCount = activeAdminCount,
            DeactivatedCount = deactivatedCount
        };

        return ServiceResult<UserStatisticsResponseDto>
            .SuccessResult(response);
    }
    public async Task<ServiceResult<UserResponseDto>> GetUserByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return ServiceResult<UserResponseDto>.ErrorResult(
                "User not found."
            );
        }

        var roles = await _userManager.GetRolesAsync(user);

        var dto = _mapper.Map<UserResponseDto>(user);
        dto.Roles = roles.ToList();

        return ServiceResult<UserResponseDto>.SuccessResult(dto);
    }
    
    public async Task<ServiceResult<bool>> ChangeUserPasswordAsync(
        Guid id,
        ChangeUserPasswordRequestDto request)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user is null)
        {
            return ServiceResult<bool>.ErrorResult(
                "User not found."
            );
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var result = await _userManager.ResetPasswordAsync(
            user,
            token,
            request.NewPassword
        );

        if (!result.Succeeded)
        {
            var errors = result.Errors
                .Select(e => e.Description)
                .ToList();

            return ServiceResult<bool>.ErrorResult(
                "Failed to change user password.",
                errors
            );
        }

        return ServiceResult<bool>.SuccessResult(
            true,
            "User password changed successfully."
        );
    }
    
    public async Task<ServiceResult<UserResponseDto>> UpdateMyProfileAsync(
        Guid id,
        UpdateFullNameRequestDto request)
    {
        var user = await _userManager.FindByIdAsync(
            id.ToString()
        );

        if (user is null)
        {
            return ServiceResult<UserResponseDto>.ErrorResult(
                "User not found."
            );
        }

        user.FullName = request.FullName;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = result.Errors
                .Select(e => e.Description)
                .ToList();

            return ServiceResult<UserResponseDto>.ErrorResult(
                "Failed to update profile.",
                errors
            );
        }

        var roles = await _userManager.GetRolesAsync(user);

        var response = _mapper.Map<UserResponseDto>(user);
        response.Roles = roles.ToList();

        return ServiceResult<UserResponseDto>.SuccessResult(
            response,
            "Profile updated successfully."
        );
    }
}