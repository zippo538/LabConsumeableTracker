namespace LabConsumableExpiryTracker.Application.DTOs.ScientistDTOs;

public class UserProfileResponseDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; }
    public string Role { get; set; } = string.Empty;
    public string IsActive { get; set; } = string.Empty;
}