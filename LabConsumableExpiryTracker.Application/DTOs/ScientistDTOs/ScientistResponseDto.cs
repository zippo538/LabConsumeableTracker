namespace LabConsumableExpiryTracker.Application.DTOs.ScientistDTOs;

public record ScientistResponseDto
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
