using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LabConsumableExpiryTracker.Application.DTOs.DisposalDTOs
{
    public class CreateDisposalResponseDto
    {
        public DisposalDto Disposal { get; set; } = null!;
        public LotSnapshotDto Lot { get; set; } = null!;
    }
}