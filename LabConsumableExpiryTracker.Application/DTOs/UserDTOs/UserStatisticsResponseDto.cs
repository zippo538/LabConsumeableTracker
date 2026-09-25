namespace LabConsumableExpiryTracker.Application.DTOs.UserDTOs;

public class UserStatisticsResponseDto
{
    public int TotalCount { get; init; }
    public int ActiveCount { get; init; }
    public int ActiveAdminCount { get; init; }
    public int DeactivatedCount { get; init; }
}