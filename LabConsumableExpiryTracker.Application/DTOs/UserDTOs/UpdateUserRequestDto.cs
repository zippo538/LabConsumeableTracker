namespace LabConsumableExpiryTracker.Application.DTOs.UserDTOs;

public class UpdateUserRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}