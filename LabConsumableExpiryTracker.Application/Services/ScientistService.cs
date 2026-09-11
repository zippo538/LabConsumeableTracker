using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.Configurations;
using LabConsumableExpiryTracker.Application.DTOs.ScientistDTOs;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LabConsumableExpiryTracker.Application.Services;

public class ScientistService : IScientistService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly ILogger<ScientistService> _logger;

    public ScientistService(
        UserManager<ApplicationUser> userManager,
        IOptions<JwtSettings> jwtSettings,
        IMapper mapper, ILogger<ScientistService> logger)
    {
        _userManager = userManager;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ServiceResult<ScientistResponseDto>> RegisterScientistAsync(
        CreateScientistRequestDto createScientistRequest)
    {
        var user = new ApplicationUser
        {
            UserName = createScientistRequest.Username,
            FullName = createScientistRequest.Fullname,
            Email = createScientistRequest.Email,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(
            user,
            createScientistRequest.Password
        );

        if (!result.Succeeded)
        {
            var errors = result.Errors
                .Select(e => e.Description)
                .ToList();
            _logger.LogWarning(
                "Failed to register scientist {Username}.",
                createScientistRequest.Username);

            return ServiceResult<ScientistResponseDto>.ErrorResult(
                "Failed to create scientist.",
                errors
            );
        }

        var roleResult = await _userManager.AddToRoleAsync(
            user,
            UserRole.Scientist.ToString()
        );

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            var errors = roleResult.Errors
                .Select(e => e.Description)
                .ToList();

            _logger.LogWarning(
                "Failed to assign scientist role to {Username}.",
                user.UserName);


            return ServiceResult<ScientistResponseDto>.ErrorResult(
                "Failed to assign scientist role.",
                errors
            );
        }

        _logger.LogInformation(
            "Scientist {Username} registered successfully.",
            user.UserName);

        var response = _mapper.Map<ScientistResponseDto>(user);

        return ServiceResult<ScientistResponseDto>.SuccessResult(response);
    }

    public async Task<ServiceResult<List<ScientistResponseDto>>> GetAllScientistsAsync()
    {
        var scientists = await _userManager.GetUsersInRoleAsync(UserRole.Scientist.ToString());

        var response = _mapper.Map<List<ScientistResponseDto>>(scientists);
        foreach (var user in response)
        {
            user.Role = UserRole.Scientist.ToString();
        }


        return ServiceResult<List<ScientistResponseDto>>.SuccessResult(response);
    }

    public async Task<ServiceResult<ScientistResponseDto>> GetUserProfileAsync(
        Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null || !user.IsActive)
        {
            _logger.LogWarning(
                "Failed to retrieve user profile for {UserId}: user not found or inactive.",
                userId);

            return ServiceResult<ScientistResponseDto>.ErrorResult(
                "User not found or inactive."
            );
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? string.Empty;

        var response = _mapper.Map<ScientistResponseDto>(user);
        response.Role = role;

        return ServiceResult<ScientistResponseDto>.SuccessResult(
            response,
            "User profile retrieved successfully."
        );
    }

    public async Task<ServiceResult<ScientistResponseDto>> UpdateScientistAsync(
        Guid id,
        UpdateScientistRequestDto requestDto)
    {
        var scientist = await _userManager.FindByIdAsync(id.ToString());

        if (scientist is null ||
            !await _userManager.IsInRoleAsync(scientist, UserRole.Scientist.ToString()))
        {
            _logger.LogWarning(
                "Scientist not found.");

            return ServiceResult<ScientistResponseDto>.ErrorResult(
                "Scientist not found."
            );
        }

        scientist.UserName = requestDto.Username;
        scientist.Email = requestDto.Email;
        scientist.FullName = requestDto.FullName;

        var result = await _userManager.UpdateAsync(scientist);

        if (!result.Succeeded)
        {
            var errors = result.Errors
                .Select(e => e.Description)
                .ToList();

            _logger.LogWarning(
                "Failed to update scientist {Username}.",
                scientist.UserName);

            return ServiceResult<ScientistResponseDto>.ErrorResult(
                "Failed to update scientist detail.",
                errors
            );
        }

        _logger.LogInformation(
            "Scientist {Username} updated successfully.",
            scientist.UserName);

        var response = _mapper.Map<ScientistResponseDto>(scientist);


        return ServiceResult<ScientistResponseDto>.SuccessResult(response);
    }

    public async Task<ServiceResult<bool>> SetScientistActiveStatusAsync(
        Guid id,
        bool isActive)
    {
        var scientist = await _userManager.FindByIdAsync(id.ToString());

        if (scientist is null ||
            !await _userManager.IsInRoleAsync(
                scientist,
                UserRole.Scientist.ToString()))
        {
            _logger.LogWarning(
                "Scientist not found.");
            return ServiceResult<bool>.ErrorResult(
                "Scientist not found."
            );
        }

        scientist.IsActive = isActive;

        var result = await _userManager.UpdateAsync(scientist);

        if (!result.Succeeded)
        {
            var errors = result.Errors
                .Select(e => e.Description)
                .ToList();

            _logger.LogWarning(
                "Failed to update scientist status.");
            return ServiceResult<bool>.ErrorResult(
                "Failed to update scientist status.",
                errors
            );
        }

        _logger.LogInformation(
            "Scientist {Username} status changed to {IsActive}.",
            scientist.UserName,
            isActive);

        return ServiceResult<bool>.SuccessResult(true);
    }
}