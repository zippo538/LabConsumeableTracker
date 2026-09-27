using AutoMapper;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.DTOs.UserDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Infrastructure.Models;
using LabConsumableExpiryTracker.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LabConsumableExpiryTracker.Infrastructure.Services;

public class UserService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    IRefreshTokenRepository refreshTokenRepository,
    IMapper mapper,
    ILogger<UserService> logger)
    : IUserService
{
    public async Task<ServiceResult<UserResponseDto>> CreateUserAsync(CreateUserRequestDto request)
    {
        var roleName = request.Role.ToString();

        if (!await roleManager.RoleExistsAsync(roleName))
        {
            return ServiceResult<UserResponseDto>.ErrorResult($"Role '{roleName}' does not exist.");
        }

        var user = mapper.Map<ApplicationUser>(request);

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            logger.LogWarning("Failed to create user {Username}.", request.Username);
            return ServiceResult<UserResponseDto>.ErrorResult("Failed to create user.", errors);
        }

        var roleResult = await userManager.AddToRoleAsync(user, roleName);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            var errors = roleResult.Errors.Select(e => e.Description).ToList();
            logger.LogWarning("Failed to assign role {Role} to {Username}.", roleName, user.UserName);
            return ServiceResult<UserResponseDto>.ErrorResult("Failed to assign user role.", errors);
        }

        logger.LogInformation("User {Username} created successfully with role {Role}.", user.UserName, roleName);

        var response = mapper.Map<UserResponseDto>(user);
        response.Roles = [roleName];

        return ServiceResult<UserResponseDto>.SuccessResult(response, "User created successfully.");
    }

    public async Task<ServiceResult<UserResponseDto>> GetUserProfileAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null || !user.IsActive)
        {
            logger.LogWarning("User {UserId} not found or inactive.", userId);
            return ServiceResult<UserResponseDto>.ErrorResult("User not found or inactive.");
        }

        var roles = await userManager.GetRolesAsync(user);
        var response = mapper.Map<UserResponseDto>(user);
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

        var query = userManager.Users
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
                u.FullName.Contains(keyword)
            );
        }

        // filter status
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (status.Equals("active", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(u => u.IsActive);
            }
            else if (status.Equals("inactive", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(u => !u.IsActive);
            }
        }

        // filter role
        if (!string.IsNullOrWhiteSpace(role))
        {
            var usersInRole = await userManager.GetUsersInRoleAsync(role);
            var userIds = usersInRole.Select(u => u.Id).ToList();
            query = query.Where(u => userIds.Contains(u.Id));
        }

        var totalCount = await query.CountAsync();

        var adminUsers = await userManager.GetUsersInRoleAsync(nameof(UserRole.WarehouseAdmin));
        var adminIds = adminUsers.Select(u => u.Id).ToHashSet();

        var users = await query
            .OrderByDescending(u => adminIds.Contains(u.Id))
            .ThenBy(u => u.UserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var resultList = new List<UserResponseDto>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            var dto = mapper.Map<UserResponseDto>(user);
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

        return ServiceResult<PagedUsersResponseDto>.SuccessResult(response);
    }

    public async Task<ServiceResult<UserResponseDto>> UpdateUserAsync(
        Guid id,
        UpdateUserRequestDto request)
    {
        var user = await userManager.FindByIdAsync(id.ToString());

        if (user is null)
        {
            return ServiceResult<UserResponseDto>.ErrorResult("User not found.");
        }

        user.FullName = request.FullName;

        var usernameResult = await userManager.SetUserNameAsync(user, request.Username);
        if (!usernameResult.Succeeded)
        {
            var errors = usernameResult.Errors.Select(e => e.Description).ToList();
            return ServiceResult<UserResponseDto>.ErrorResult("Failed to update username.", errors);
        }

        var emailResult = await userManager.SetEmailAsync(user, request.Email);
        if (!emailResult.Succeeded)
        {
            var errors = emailResult.Errors.Select(e => e.Description).ToList();
            return ServiceResult<UserResponseDto>.ErrorResult("Failed to update email.", errors);
        }

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = updateResult.Errors.Select(e => e.Description).ToList();
            return ServiceResult<UserResponseDto>.ErrorResult("Failed to update user.", errors);
        }

        var roles = await userManager.GetRolesAsync(user);

        var response = mapper.Map<UserResponseDto>(user);
        response.Roles = roles.ToList();

        return ServiceResult<UserResponseDto>.SuccessResult(response, "User updated successfully.");
    }

    public async Task<ServiceResult<bool>> SetUserActiveStatusAsync(
        Guid currentAdminId,
        Guid targetUserId,
        bool isActive)
    {
        if (currentAdminId == targetUserId && !isActive)
        {
            return ServiceResult<bool>.ErrorResult("You cannot deactivate your own account.");
        }

        var user = await userManager.FindByIdAsync(targetUserId.ToString());
        if (user is null)
        {
            return ServiceResult<bool>.ErrorResult("User not found.");
        }

        user.IsActive = isActive;
        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ServiceResult<bool>.ErrorResult("Failed to update user status.", errors);
        }

        if (!isActive)
        {
            await refreshTokenRepository.RevokeAllByUserIdAsync(targetUserId);
        }

        logger.LogInformation("User {Username} status changed to {IsActive}.", user.UserName, isActive);
        return ServiceResult<bool>.SuccessResult(true, $"User {(isActive ? "activated" : "deactivated")} successfully.");
    }

    public async Task<ServiceResult<UserStatisticsResponseDto>> GetUserStatisticsAsync()
    {
        var totalCount = await userManager.Users.CountAsync();
        var activeCount = await userManager.Users.CountAsync(u => u.IsActive);
        var deactivatedCount = await userManager.Users.CountAsync(u => !u.IsActive);

        var adminUsers = await userManager.GetUsersInRoleAsync(nameof(UserRole.WarehouseAdmin));
        var activeAdminCount = adminUsers.Count(u => u.IsActive);

        var response = new UserStatisticsResponseDto
        {
            TotalCount = totalCount,
            ActiveCount = activeCount,
            ActiveAdminCount = activeAdminCount,
            DeactivatedCount = deactivatedCount
        };

        return ServiceResult<UserStatisticsResponseDto>.SuccessResult(response);
    }

    public async Task<ServiceResult<UserResponseDto>> GetUserByIdAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);

        if (user == null)
        {
            return ServiceResult<UserResponseDto>.ErrorResult("User not found.");
        }

        var roles = await userManager.GetRolesAsync(user);

        var dto = mapper.Map<UserResponseDto>(user);
        dto.Roles = roles.ToList();

        return ServiceResult<UserResponseDto>.SuccessResult(dto);
    }

    public async Task<ServiceResult<bool>> ChangeUserPasswordAsync(
        Guid id,
        ChangeUserPasswordRequestDto request)
    {
        var user = await userManager.FindByIdAsync(id.ToString());

        if (user is null)
        {
            return ServiceResult<bool>.ErrorResult("User not found.");
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        var result = await userManager.ResetPasswordAsync(user, token, request.NewPassword);

        if (result.Succeeded)
        {
            return ServiceResult<bool>.SuccessResult(true, "User password changed successfully.");
        }

        var errors = result.Errors.Select(e => e.Description).ToList();
        return ServiceResult<bool>.ErrorResult("Failed to change user password.", errors);
    }

    public async Task<ServiceResult<UserResponseDto>> UpdateMyProfileAsync(
        Guid id,
        UpdateFullNameRequestDto request)
    {
        var user = await userManager.FindByIdAsync(id.ToString());

        if (user is null)
        {
            return ServiceResult<UserResponseDto>.ErrorResult("User not found.");
        }

        user.FullName = request.FullName;

        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ServiceResult<UserResponseDto>.ErrorResult("Failed to update profile.", errors);
        }

        var roles = await userManager.GetRolesAsync(user);

        var response = mapper.Map<UserResponseDto>(user);
        response.Roles = roles.ToList();

        return ServiceResult<UserResponseDto>.SuccessResult(response, "Profile updated successfully.");
    }
}
