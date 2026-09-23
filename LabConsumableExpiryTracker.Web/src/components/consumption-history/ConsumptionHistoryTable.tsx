import { useNavigate } from "react-router";

import type { ConsumptionRecordDto } from "../../types/consumptionHistory.types";
import SkeleteonRow from "./SkeleteonRow";

interface ConsumptionHistoryTableProps {
  items: ConsumptionRecordDto[];
  isLoading: boolean;
  isError: boolean;
  userName: string;
  formatQuantity: (quantity: number) => string;
  formatUnit: (baseUnit: string) => string;
  formatConsumedAt: (isoDate: string) => string;
  toConsumptionRef: (id: string) => string;
}

export default function ConsumptionHistoryTable({
  items,
  isLoading,
  isError,
  userName,
  formatQuantity,
  formatUnit,
  formatConsumedAt,
  toConsumptionRef,
}: ConsumptionHistoryTableProps) {
  const navigate = useNavigate();

  return (
    <div className="overflow-hidden rounded-xl border border-slate-200 bg-white">
      <div className="overflow-x-auto">
        <table className="w-full min-w-[800px] text-left text-sm">
          <thead>
            <tr className="border-b border-slate-100 text-xs font-medium uppercase tracking-wide text-slate-400">
              <th className="px-6 py-3">Consumption / Actor</th>
              <th className="px-6 py-3">Job</th>
              <th className="px-6 py-3">Item / Lot</th>
              <th className="px-6 py-3">Quantity</th>
              <th className="px-6 py-3">Consumed At</th>
              <th className="px-6 py-3 text-right">Detail</th>
            </tr>
          </thead>

          <tbody className="divide-y divide-slate-100">
            {isLoading &&
              Array.from({ length: 5 }).map((_, index) => (
                <SkeleteonRow key={index} />
              ))}

            {!isLoading && isError && (
              <tr>
                <td colSpan={6} className="px-6 py-10 text-center">
                  <p className="text-sm font-medium text-slate-700">
                    Couldn't load consumption history
                  </p>

                  <p className="mt-1 text-sm text-slate-400">
                    Check your connection and try again.
                  </p>
                </td>
              </tr>
            )}

            {!isLoading && !isError && items.length === 0 && (
              <tr>
                <td colSpan={6} className="px-6 py-10 text-center">
                  <p className="text-sm font-medium text-slate-700">
                    No consumption records found
                  </p>

                  <p className="mt-1 text-sm text-slate-400">
                    Try adjusting your search or filters.
                  </p>
                </td>
              </tr>
            )}

            {!isLoading &&
              !isError &&
              items.map((record) => (
                <tr
                  key={record.id}
                  className="hover:bg-slate-50"
                >
                  <td className="px-6 py-4">
                    <p className="font-semibold text-teal-700">
                      {toConsumptionRef(record.id)}
                    </p>

                    <p className="text-xs text-slate-400">
                      {userName}
                    </p>
                  </td>

                  <td className="px-6 py-4">
                    <span className="inline-flex items-center rounded-full bg-green-100 px-2.5 py-1 text-xs font-medium text-slate-600">
                      {record.jobNumber}
                    </span>
                  </td>

                  <td className="px-6 py-4 text-slate-600">
                    <p>{record.itemName}</p>
                    <p className="text-xs text-slate-400">
                      {record.lotNumber}
                    </p>
                  </td>

                  <td className="px-6 py-4 text-slate-600">
                    {formatQuantity(record.quantity)}{" "}
                    {formatUnit(record.baseUnit)}
                  </td>

                  <td className="px-6 py-4 text-slate-600">
                    {formatConsumedAt(record.consumedAt)}
                  </td>

                  <td className="px-6 py-4 text-right">
                    <button
                      type="button"
                      onClick={() =>
                        navigate(
                          `/app/consumptions-history/${record.id}`
                        )
                      }
                      className="rounded-lg bg-teal-800 px-4 py-1.5 text-xs font-semibold text-white hover:bg-teal-900"
                    >
                      View
                    </button>
                  </td>
                </tr>
              ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}