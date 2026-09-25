namespace LabConsumableExpiryTracker.Application.DTOs.UserDTOs;

public class UpdateUserRequestDto
{
    public string Username { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}