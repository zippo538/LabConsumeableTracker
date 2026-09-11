namespace LabConsumableExpiryTracker.Application.DTOs.Scientist;

public record UpdateScientistRequestDto(
    string Username,
    string FullName,
    string Email
);