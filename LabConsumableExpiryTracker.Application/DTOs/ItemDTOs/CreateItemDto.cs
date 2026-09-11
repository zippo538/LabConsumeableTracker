using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.DTOs.ItemDTOs
{
    public class CreateItemDto
    {
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public UnitOfMeasure BaseUnit { get; set; }

        public decimal MinimumStock { get; set; }

        public int ExpiringSoonDays { get; set; }

    }
}