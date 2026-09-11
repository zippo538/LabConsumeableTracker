namespace LabConsumableExpiryTracker.Application.DTOs.ScientistDTOs;

public class CreateScientistRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Fullname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}