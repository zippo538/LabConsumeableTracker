namespace LabConsumableExpiryTracker.Application.DTOs.UserDTOs;

public class UserStatisticsResponseDto
{
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int ActiveAdminCount { get; set; }
    public int DeactivatedCount { get; set; }
}