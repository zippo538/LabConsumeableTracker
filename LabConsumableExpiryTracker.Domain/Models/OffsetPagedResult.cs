using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LabConsumableExpiryTracker.Domain.Models
{
    public class OffsetPagedResult<T>
    {
        public required IReadOnlyList<T> Items { get; init; }

        public required int Offset { get; init; }

        public required int Limit { get; init; }

        public required int TotalCount { get; init; }

        public bool HasNext =>
            Offset + Items.Count < TotalCount;

        public int NextOffset =>
            HasNext ? Offset + Limit : Offset;
    }
}