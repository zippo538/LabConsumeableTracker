namespace LabConsumableExpiryTracker.Application.DTOs.DashboardDTOs;

public class DashboardSummaryDto
{
    public int LowStockItemCount { get; init; }
    public int ValidLotCount { get; init; }
    public int ExpiringSoonLotCount { get; init; }
    public int ExpiredLotCount { get; init; }
    public int BlockedLotCount { get; init; }
    public DateTimeOffset EvaluatedAt { get; init; }
}