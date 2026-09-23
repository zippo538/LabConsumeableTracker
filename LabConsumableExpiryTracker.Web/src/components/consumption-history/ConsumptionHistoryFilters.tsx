import { Search } from "lucide-react";

interface ConsumptionHistoryFiltersProps {
  lotNumber: string;
  jobId: string;
  itemId: string;
  dateRange: string;

  jobOptions: [string, string][];
  itemOptions: [string, string][];

  dateRangeOptions: readonly {
    label: string;
    value: string;
  }[];

  onLotNumberChange: (value: string) => void;
  onJobChange: (value: string) => void;
  onItemChange: (value: string) => void;
  onDateRangeChange: (value: string) => void;
}

export default function ConsumptionHistoryFilters({
  lotNumber,
  jobId,
  itemId,
  dateRange,
  jobOptions,
  itemOptions,
  dateRangeOptions,
  onLotNumberChange,
  onJobChange,
  onItemChange,
  onDateRangeChange,
}: ConsumptionHistoryFiltersProps) {
  return (
    <div className="flex flex-col gap-3 rounded-xl border border-slate-200 bg-white p-3 sm:flex-row sm:items-center">
      <div className="relative flex-1">
        <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />

        <input
          type="text"
          value={lotNumber}
          onChange={(event) => onLotNumberChange(event.target.value)}
          placeholder="Search lot number..."
          className="w-full rounded-lg border border-slate-200 py-2 pl-9 pr-3 text-sm text-slate-700 placeholder:text-slate-400 focus:border-teal-500 focus:outline-none focus:ring-1 focus:ring-teal-500"
        />
      </div>

      <select
        value={jobId}
        onChange={(event) => onJobChange(event.target.value)}
        className="rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-700 focus:border-teal-500 focus:outline-none focus:ring-1 focus:ring-teal-500"
      >
        <option value="">All jobs</option>

        {jobOptions.map(([id, number]) => (
          <option key={id} value={id}>
            {number}
          </option>
        ))}
      </select>

      <select
        value={itemId}
        onChange={(event) => onItemChange(event.target.value)}
        className="rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-700 focus:border-teal-500 focus:outline-none focus:ring-1 focus:ring-teal-500"
      >
        <option value="">All items</option>

        {itemOptions.map(([id, name]) => (
          <option key={id} value={id}>
            {name}
          </option>
        ))}
      </select>

      <select
        value={dateRange}
        onChange={(event) => onDateRangeChange(event.target.value)}
        className="rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-700 focus:border-teal-500 focus:outline-none focus:ring-1 focus:ring-teal-500"
      >
        {dateRangeOptions.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    </div>
  );
}