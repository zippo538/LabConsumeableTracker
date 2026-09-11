using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using LabConsumableExpiryTracker.Application.Common.Result;
using LabConsumableExpiryTracker.Application.Configurations;
using LabConsumableExpiryTracker.Application.DTOs.Scientist;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Domain.Enums;
using LabConsumableExpiryTracker.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LabConsumableExpiryTracker.Application.Services;

public class AuthService :IScientistService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtSettings _jwtSettings;
    private readonly IMapper _mapper;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IOptions<JwtSettings> jwtSettings,
        IMapper mapper)
    {
        _userManager = userManager;
        _jwtSettings = jwtSettings.Value;
        _mapper = mapper;
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

            return ServiceResult<ScientistResponseDto>.ErrorResult(
                "Failed to assign scientist role.",
                errors
            );
        }

        var response = _mapper.Map<ScientistResponseDto>(user);

        return ServiceResult<ScientistResponseDto>.SuccessResult(response);
    }

    public async Task<ServiceResult<List<UserProfileResponseDto>>> GetAllScientistsAsync()
    {
        var scientists = await _userManager.GetUsersInRoleAsync(UserRole.Scientist.ToString());

        var response = _mapper.Map<List<UserProfileResponseDto>>(scientists);
        foreach (var user in response)
        {
            user.Role = UserRole.Scientist.ToString();
        }


        return ServiceResult<List<UserProfileResponseDto>>.SuccessResult(response);
    }
    
    public async Task<ServiceResult<UserProfileResponseDto>> GetUserProfileAsync(
        Guid userId,
        CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null || !user.IsActive)
        {
            return ServiceResult<UserProfileResponseDto>.ErrorResult(
                "User not found or inactive."
            );
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? string.Empty;

        var response = _mapper.Map<UserProfileResponseDto>(user);
        response.Role = role;

        return ServiceResult<UserProfileResponseDto>.SuccessResult(
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

            return ServiceResult<ScientistResponseDto>.ErrorResult(
                "Failed to update scientist detail.",
                errors
            );
        }

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

            return ServiceResult<bool>.ErrorResult(
                "ErrorResulted to update scientist status.",
                errors
            );
        }

        return ServiceResult<bool>.SuccessResult(true);
    }

    public async Task<ServiceResult<bool>> DeleteScientistAsync(Guid id)
    {
        var scientist = await _userManager.FindByIdAsync(id.ToString());

        if (scientist is null || !await _userManager.IsInRoleAsync(scientist, UserRole.Scientist.ToString()))
        {
            return ServiceResult<bool>.ErrorResult(
                "Scientist tidak ditemukan."
            );
        }

        var result = await _userManager.DeleteAsync(scientist);

        if (!result.Succeeded)
        {
            var errors = result.Errors
                .Select(e => e.Description)
                .ToList();

            return ServiceResult<bool>.ErrorResult(
                "ErrorResulted to delete scientist account.",
                errors
            );
        }

        return ServiceResult<bool>.SuccessResult(true);
    }

    private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);


        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Email, user.Email!)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)
        );

        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

  
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                _jwtSettings.ExpiryHours
            ),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}