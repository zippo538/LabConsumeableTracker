// src/components/lots/LotFilterBar.tsx
import type { ItemOption } from "../../types/lot.types";

interface LotFilterBarProps {
  search: string;
  onSearchChange: (value: string) => void;
  filterItem: string;
  onFilterItemChange: (value: string) => void;
  filterStatus: string;
  onFilterStatusChange: (value: string) => void;
  items: ItemOption[];
  onRegisterNew: () => void;
  canRegister: boolean;
}

export const LotFilterBar = ({
  search,
  onSearchChange,
  filterItem,
  onFilterItemChange,
  filterStatus,
  onFilterStatusChange,
  items,
  onRegisterNew,
  canRegister,
}: LotFilterBarProps) => {
  return (
    <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
      {/* Search */}
      <div className="flex-1">
        <input
          type="text"
          placeholder="Search Lot number..."
          value={search}
          onChange={(e) => onSearchChange(e.target.value)}
          className="w-full rounded-lg border border-slate-200 px-4 py-2 text-sm focus:border-teal-500 focus:outline-none focus:ring-2 focus:ring-teal-100"
        />
      </div>

      {/* Item Filter */}
      <select
        value={filterItem}
        onChange={(e) => onFilterItemChange(e.target.value)}
        className="rounded-lg border border-slate-200 px-4 py-2 text-sm focus:border-teal-500 focus:outline-none focus:ring-2 focus:ring-teal-100"
      >
        <option value="">All Items</option>
        {items.map((item) => (
          <option key={item.id} value={item.id}>
            {item.code} - {item.name}
          </option>
        ))}
      </select>

      {/* Status Filter */}
      <select
        value={filterStatus}
        onChange={(e) => onFilterStatusChange(e.target.value)}
        className="rounded-lg border border-slate-200 px-4 py-2 text-sm focus:border-teal-500 focus:outline-none focus:ring-2 focus:ring-teal-100"
      >
        <option value="">Status / Expiry</option>
        <option value="Active">Active</option>
        <option value="ExpiringSoon">Expiring Soon</option>
        <option value="Expired">Expired</option>
        <option value="Quarantined">Quarantined</option>
        <option value="ManuallyBlocked">Blocked</option>
      </select>

      {/* Register Button */}
      {canRegister && (
        <button
          onClick={onRegisterNew}
          className="rounded-lg bg-green-500 px-4 py-2 text-sm font-semibold text-white transition hover:bg-green-600 focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-offset-2"
        >
          + Register New Lot
        </button>
      )}
    </div>
  );
};
