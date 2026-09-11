namespace LabConsumableExpiryTracker.Application.DTOs.ScientistDTOs;
public class UpdateScientistRequestDto
{ 
    public string Username { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;
    public string FullName { get; set; }
}
