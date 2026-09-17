using LabConsumableExpiryTracker.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace LabConsumableExpiryTracker.Application.DTOs.ItemDTOs
{
    public class UpdateItemDto
    {
        public string Name { get; set; } = string.Empty;
        public UnitOfMeasure BaseUnit { get; set; }
        public decimal MinimumStock { get; set; }
        public int ExpiringSoonDays { get; set; }

        public IFormFile? Image { get; set; }
    }
}