namespace LabConsumableExpiryTracker.Application.DTOs.UserDTOs;

public class UserResponseDto
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public bool IsActive { get; init; }
}