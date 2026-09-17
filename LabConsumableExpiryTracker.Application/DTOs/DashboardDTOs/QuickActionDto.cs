namespace LabConsumableExpiryTracker.Application.DTOs.DashboardDTOs;

public sealed class QuickActionDto
{
    public string Code { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Href { get; init; } = string.Empty;
}
