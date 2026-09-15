using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LabConsumableExpiryTracker.Application.DTOs.OffsetPaginationDTOs
{
    public class OffsetPaginationRequest
    {
        public int Offset { get; init; } = 0;
        public int Limit { get; init; } = 20;
    }
}