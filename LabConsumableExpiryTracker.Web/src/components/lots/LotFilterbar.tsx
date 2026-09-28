// src/components/lots/LotFilterBar.tsx
import type { ItemOption } from "../../types/lot.types";

interface LotFilterBarProps {
  search: string;
  onSearchChange: (value: string) => void;
  filterItem: string;
  onFilterItemChange: (value: string) => void;
  filterAdministrativeStatus: string;
  onFilterAdministrativeStatusChange: (value: string) => void;
  filterExpiryCondition: string;
  onFilterExpiryConditionChange: (value: string) => void;
  items: ItemOption[];
  onRegisterNew: () => void;
  canRegister: boolean;
}

export const LotFilterBar = ({
  search,
  onSearchChange,
  filterItem,
  onFilterItemChange,
  filterAdministrativeStatus,
  onFilterAdministrativeStatusChange,
  filterExpiryCondition,
  onFilterExpiryConditionChange,
  items,
  onRegisterNew,
  canRegister,
}: LotFilterBarProps) => {
  return (
    <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
   
      <div className="w-30">
        <input
          type="text"
          placeholder="Search Lot number..."
          value={search}
          onChange={(e) => onSearchChange(e.target.value)}
          className="w-full rounded-lg border border-slate-200 px-4 py-2 text-sm focus:border-teal-500 focus:outline-none focus:ring-2 focus:ring-teal-100"
        />
      </div>


      <div className="">
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
      </div>

      <select
        value={filterAdministrativeStatus}
        onChange={(e) => onFilterAdministrativeStatusChange(e.target.value)}
        className="rounded-lg border border-slate-200 px-4 py-2 text-sm focus:border-teal-500 focus:outline-none focus:ring-2 focus:ring-teal-100"
      >
        <option value="">All Status</option>
        <option value="Active">Active</option>
        <option value="Quarantined">Quarantined</option>
        <option value="ManuallyBlocked">Manually Blocked</option>
        <option value="Disposed">Disposed</option>
        <option value="Depleted">Depleted</option>
      </select>

      <select
        value={filterExpiryCondition}
        onChange={(e) => onFilterExpiryConditionChange(e.target.value)}
        className="rounded-lg border border-slate-200 px-4 py-2 text-sm focus:border-teal-500 focus:outline-none focus:ring-2 focus:ring-teal-100"
      >
        <option value="">All Expiry</option>
        <option value="Valid">Valid</option>
        <option value="ExpiringSoon">Expiring Soon</option>
        <option value="Expired">Expired</option>
      </select>

      {canRegister && (
        <button
          onClick={onRegisterNew}
          className="rounded-lg bg-[#0EA5A5] hover:bg-[#1dbaba] px-2 py-3 text-[12px] font-semibold text-white transition focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-offset-2"
        >
          + Register New Lot
        </button>
      )}
    </div>
  );
};