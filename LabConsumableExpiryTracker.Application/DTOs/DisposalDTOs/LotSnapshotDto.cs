using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LabConsumableExpiryTracker.Application.DTOs.DisposalDTOs
{
    public class LotSnapshotDto
    {
        public string Id { get; set; } = string.Empty;
        public decimal RemainingQuantity { get; set; }
        public string AdministrativeStatus { get; set; } = string.Empty;
        public string RowVersion { get; set; } = string.Empty; 
        public string? EvaluatedAt { get; set; }
    }
}