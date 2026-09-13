using LabConsumableExpiryTracker.Domain.Enums;

namespace LabConsumableExpiryTracker.Application.DTOs.LotDTOs
{
    public class LotDto
    {
        public Guid Id { get; init; }
        public Guid ItemId { get; init; }
        public string LotNumber { get; init; } = null!;
        public DateTimeOffset ReceivedAt { get; init; }
        public decimal InitialQuantity { get; init; }
        public decimal RemainingQuantity { get; set; }
        public DateOnly ExpiryDate { get; init; }
        public string StorageLocation { get; init; } = null!;
        public LotAdministrativeStatus AdministrativeStatus { get; set; }
        public ExpiryCondition ExpiryCondition { get; set; }
        public bool IsEligible { get; set; }
        public DateTimeOffset EvaluatedAt { get; set; }

        public byte[] RowVersion { get; init; } = [];

    }
}