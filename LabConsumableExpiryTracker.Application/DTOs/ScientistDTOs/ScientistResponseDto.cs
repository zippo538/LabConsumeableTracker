namespace LabConsumableExpiryTracker.Application.DTOs.ScientistDTOs;

public class ScientistResponseDto
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;
    public string FullName { get; set; }
    public string Role { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}
