// src/components/lots/LotTable.tsx
import type { Lot } from "../../types/lot.types";
import { LotStatusBadge } from "./LotStatusBadge";

interface LotTableProps {
  lots: Lot[];
  isLoading: boolean;
  onUpdate: (lot: Lot) => void;
  onDelete: (lot: Lot) => void;
  canEdit: boolean;
}

export const LotTable = ({
  lots,
  isLoading,
  onUpdate,
  onDelete,
  canEdit,
}: LotTableProps) => {
  if (isLoading) {
    return (
      <div className="rounded-lg border border-slate-200 bg-white p-8 text-center">
        <div className="mx-auto h-8 w-8 animate-spin rounded-full border-2 border-teal-200 border-t-teal-600" />
        <p className="mt-2 text-sm text-slate-500">Loading lots...</p>
      </div>
    );
  }

  if (lots.length === 0) {
    return (
      <div className="rounded-lg border border-slate-200 bg-white p-8 text-center">
        <p className="text-sm text-slate-500">No lots found.</p>
      </div>
    );
  }

  return (
    <div className="overflow-x-auto rounded-lg border border-slate-200 bg-white">
      <table className="w-full">
        <thead>
          <tr className="border-b border-slate-200 bg-slate-50">
            <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
              Item / Lot
            </th>
            <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
              Remaining / Initial
            </th>
            <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
              Expiry
            </th>
            <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
              Location
            </th>
            <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
              Status
            </th>
            {canEdit && (
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
                Actions
              </th>
            )}
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100">
          {lots.map((lot) => (
            <tr key={lot.id} className="transition hover:bg-slate-50">
              <td className="px-4 py-3">
                <div>
                  <p className="text-sm font-semibold text-slate-800">
                    {lot.itemName}
                  </p>
                  <p className="text-xs text-slate-500">{lot.lotNumber}</p>
                </div>
              </td>
              <td className="px-4 py-3">
                <p className="text-sm font-medium text-slate-800">
                  {lot.remainingQuantity} / {lot.initialQuantity} {lot.baseUnit}
                </p>
              </td>
              <td className="px-4 py-3">
                <p className="text-sm text-slate-800">
                  {new Date(lot.expiryDate).toLocaleDateString("en-US", {
                    day: "numeric",
                    month: "short",
                    year: "numeric",
                  })}
                </p>
              </td>
              <td className="px-4 py-3">
                <p className="text-sm text-slate-800">{lot.storageLocation}</p>
              </td>
              <td className="px-4 py-3">
                <LotStatusBadge status={lot.expiryCondition} />
              </td>
              {canEdit && (
                <td className="px-4 py-3">
                  <div className="flex gap-2">
                    <button
                      onClick={() => onUpdate(lot)}
                      className="rounded px-2 py-1 text-xs font-medium text-teal-600 hover:bg-teal-50"
                    >
                      Edit
                    </button>
                    <button
                      onClick={() => onDelete(lot)}
                      className="rounded px-2 py-1 text-xs font-medium text-red-600 hover:bg-red-50"
                    >
                      Delete
                    </button>
                  </div>
                </td>
              )}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};
