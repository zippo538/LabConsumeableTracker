namespace LabConsumableExpiryTracker.Application.DTOs.UserDTOs;

public class PagedUsersResponseDto
{
    public IReadOnlyList<UserResponseDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}