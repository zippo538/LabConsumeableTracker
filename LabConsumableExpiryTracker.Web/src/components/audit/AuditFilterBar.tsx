// ============================================================
// AuditFilterBar — Search & Filter Component
// Filter: EventType, Item, Lot, Date Range
// UI-14: Query URL hanya memuat filter aman
// ============================================================

import { type FormEvent, useCallback } from "react";
import { Search, Calendar, Filter } from "lucide-react";
import type { AuditFilter, AuditEventType, LookupOption } from "../../types/audit.types";

interface AuditFilterBarProps {
  filter: AuditFilter;
  onFilterChange: (filter: AuditFilter) => void;
  onReset: () => void;
  itemOptions: LookupOption[];
  lotOptions: LookupOption[];
  itemCountLoading?: boolean;
  lotCountLoading?: boolean;
}

/** Opsi dropdown EventType */
const EVENT_TYPE_OPTIONS: { value: AuditEventType | "All"; label: string }[] = [
  { value: "All", label: "All events" },
  { value: "Consumption", label: "Consumption" },
  { value: "Disposal", label: "Disposal" },
];

export function AuditFilterBar({
  filter,
  onFilterChange,
  onReset,
  itemOptions,
  lotOptions,
  itemCountLoading = false,
  lotCountLoading = false,
}: AuditFilterBarProps) {
  const updateFilter = useCallback(
    (partial: Partial<AuditFilter>) => {
      onFilterChange({ ...filter, ...partial, page: 1 });
    },
    [filter, onFilterChange],
  );

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    // Trigger search — page sudah direset ke 1 oleh updateFilter
  };

  const handleReset = () => {
    onReset();
  };

  return (
    <form
      onSubmit={handleSubmit}
      className="rounded-xl border border-slate-200 bg-white p-4"
    >
      <div className="flex flex-wrap items-end gap-3">
        {/* Search Input */}
        <div className="relative flex-1 min-w-[240px]">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
          <input
            type="text"
            placeholder="Search event ID, item ID, or lot number..."
            className="w-full rounded-lg border border-slate-200 bg-white py-2 pl-9 pr-3 text-sm text-slate-700 placeholder:text-slate-400 focus:border-teal-500 focus:outline-none focus:ring-1 focus:ring-teal-500"
            aria-label="Search audit events"
          />
        </div>

        {/* EventType Filter */}
        <div className="min-w-[140px]">
          <label htmlFor="eventType" className="sr-only">
            Event Type
          </label>
          <select
            id="eventType"
            value={filter.eventType}
            onChange={(e) =>
              updateFilter({
                eventType: e.target.value as AuditEventType | "All",
              })
            }
            className="w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-700 focus:border-teal-500 focus:outline-none focus:ring-1 focus:ring-teal-500"
          >
            {EVENT_TYPE_OPTIONS.map((opt) => (
              <option key={opt.value} value={opt.value}>
                {opt.label}
              </option>
            ))}
          </select>
        </div>

        {/* Item Filter */}
        <div className="min-w-[160px]">
          <label htmlFor="itemFilter" className="sr-only">
            Item
          </label>
          <select
            id="itemFilter"
            value={filter.itemId ?? ""}
            onChange={(e) => updateFilter({ itemId: e.target.value || null })}
            disabled={itemCountLoading}
            className="w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-700 focus:border-teal-500 focus:outline-none focus:ring-1 focus:ring-teal-500 disabled:opacity-50"
          >
            <option value="">All items</option>
            {itemOptions.map((opt) => (
              <option key={opt.id} value={opt.id}>
                {opt.label}
              </option>
            ))}
          </select>
        </div>

        {/* Lot Filter */}
        <div className="min-w-[160px]">
          <label htmlFor="lotFilter" className="sr-only">
            Lot
          </label>
          <select
            id="lotFilter"
            value={filter.lotId ?? ""}
            onChange={(e) => updateFilter({ lotId: e.target.value || null })}
            disabled={lotCountLoading}
            className="w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-700 focus:border-teal-500 focus:outline-none focus:ring-1 focus:ring-teal-500 disabled:opacity-50"
          >
            <option value="">All lots</option>
            {lotOptions.map((opt) => (
              <option key={opt.id} value={opt.id}>
                {opt.label}
              </option>
            ))}
          </select>
        </div>

        {/* Date Range Filter */}
        <div className="flex items-center gap-2 min-w-[200px]">
          <Calendar className="h-4 w-4 text-slate-400" />
          <input
            type="date"
            value={filter.from ?? ""}
            onChange={(e) => updateFilter({ from: e.target.value || null })}
            className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-700 focus:border-teal-500 focus:outline-none focus:ring-1 focus:ring-teal-500"
            aria-label="From date"
          />
          <span className="text-slate-400 text-sm">to</span>
          <input
            type="date"
            value={filter.to ?? ""}
            onChange={(e) => updateFilter({ to: e.target.value || null })}
            className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-700 focus:border-teal-500 focus:outline-none focus:ring-1 focus:ring-teal-500"
            aria-label="To date"
          />
        </div>

        {/* Action Buttons */}
        <div className="flex items-center gap-2">
          <button
            type="submit"
            className="inline-flex items-center gap-1.5 rounded-lg bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-700 focus:outline-none focus:ring-2 focus:ring-teal-500 focus:ring-offset-2"
          >
            <Filter className="h-4 w-4" />
            Apply
          </button>
          <button
            type="button"
            onClick={handleReset}
            className="rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-600 hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-slate-300"
          >
            Reset
          </button>
        </div>
      </div>

      {/* Helper Text (SRS: Job shown for Consumption, Reason for Disposal) */}
      <p className="mt-3 text-xs text-slate-500">
        Job is shown for Consumption; Reason is shown for Disposal. Select Event
        to view audit details.
      </p>
    </form>
  );
}
