using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LabConsumableExpiryTracker.Application.DTOs.DashboardDTOs
{
    public class DashboardSummaryDto
    {
        public int LowStockItemCount { get; set; }
        public int ExpiringSoonLotCount { get; set; }
        public int ExpiredLotCount { get; set; }
        public int BlockedLotCount { get; set; }
        public DateTimeOffset EvaluatedAt { get; set; }
    }
}