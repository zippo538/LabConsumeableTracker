namespace LabConsumableExpiryTracker.Application.DTOs.ScientistDTOs;

public record UpdateScientistRequestDto(
    string Username,
    string FullName,
    string Email
);