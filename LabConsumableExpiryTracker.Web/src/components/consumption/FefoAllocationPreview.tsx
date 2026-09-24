import React from "react";
import { Layers, ShieldCheck, AlertCircle, Loader2 } from "lucide-react";
import type { FefoAllocationPreviewDto } from "../../types/consumption.types";
import type { ItemDto } from "../../types/item.types";

interface FefoAllocationPreviewProps {
  preview: FefoAllocationPreviewDto | null;
  isLoading: boolean;
  selectedItem?: ItemDto;
}

const formatDate = (dateStr: string) => {
  if (!dateStr) return "-";
  try {
    const d = new Date(dateStr);
    return d.toLocaleDateString("en-GB", {
      day: "2-digit",
      month: "short",
      year: "numeric",
    });
  } catch {
    return dateStr;
  }
};

const formatTime = (dateStr: string) => {
  if (!dateStr) return "-";
  try {
    const d = new Date(dateStr);
    return `${d.toLocaleDateString("en-GB", {
      day: "2-digit",
      month: "short",
      year: "numeric",
    })}, ${d.toLocaleTimeString("id-ID", {
      hour: "2-digit",
      minute: "2-digit",
    })} WIB`;
  } catch {
    return dateStr;
  }
};

export const FefoAllocationPreview: React.FC<FefoAllocationPreviewProps> = ({
  preview,
  isLoading,
  selectedItem,
}) => {
  const unit = preview?.baseUnit || selectedItem?.baseUnit || "";

  return (
    <div className="flex flex-col justify-between rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
      <div>
        <div className="flex items-start justify-between border-b border-slate-100 pb-4">
          <div>
            <h2 className="text-[17px] font-bold tracking-tight text-slate-900">
              2. FEFO allocation preview
            </h2>
            <p className="mt-0.5 text-[12px] text-slate-400">
              Sorted by expiry date → received at → ID.
            </p>
          </div>

          {preview && (
            <span className="inline-flex items-center gap-1.5 rounded-full bg-cyan-50 px-3 py-1 text-[11px] font-semibold text-[#0da7ad] border border-[#b9e8eb]">
              <ShieldCheck className="h-3.5 w-3.5" />
              FEFO Verified
            </span>
          )}
        </div>

        <div className="mt-4">
          {isLoading ? (
            <div className="flex flex-col items-center justify-center py-16 text-slate-400">
              <Loader2 className="h-8 w-8 animate-spin text-[#12a9ad]" />

              <p className="mt-3 text-[13px] font-medium text-slate-600">
                Evaluating earliest expiry lots...
              </p>
            </div>
          ) : !preview ? (
            <div className="flex flex-col items-center justify-center py-16 text-center">
              <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-slate-50 text-slate-400 border border-slate-200/60">
                <Layers className="h-6 w-6" />
              </div>
              <h3 className="mt-3.5 text-[14px] font-bold text-slate-700">
                No allocation preview yet
              </h3>
              <p className="mt-1 max-w-sm text-[12px] text-slate-400 leading-relaxed">
                Select an in-progress job, choose an item from master items, enter
                the desired quantity, then click{" "}
                <strong className="text-slate-600 font-semibold">
                  Preview FEFO Allocation
                </strong>
                .
              </p>
            </div>
          ) : preview.allocations.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12 text-center">
              <AlertCircle className="h-8 w-8 text-amber-500" />
              <h3 className="mt-2 text-[14px] font-bold text-slate-800">
                No lots allocated
              </h3>
              <p className="mt-1 max-w-sm text-[12px] text-slate-500">
                There is not enough usable stock or eligible active lots for this item.
              </p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-left border-collapse">
                <thead>
                  <tr className="border-b border-slate-100 text-[11px] font-bold uppercase tracking-wider text-slate-400">
                    <th className="py-3 px-2">Order</th>
                    <th className="py-3 px-3">Lot</th>
                    <th className="py-3 px-3">Expiry</th>
                    <th className="py-3 px-3">Available remaining</th>
                    <th className="py-3 px-3">Allocated</th>
                    <th className="py-3 px-3">Location</th>
                    <th className="py-3 px-2 text-right">Status</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100 text-[12px]">
                  {preview.allocations.map((alloc, idx) => (
                    <tr key={alloc.lotId || idx} className="hover:bg-slate-50/70">
                      <td className="py-3.5 px-2 font-bold text-slate-800">
                        {idx + 1}
                      </td>
                      <td className="py-3.5 px-3 font-semibold text-[#0da7ad]">
                        {alloc.lotNumber}
                      </td>
                      <td className="py-3.5 px-3 text-slate-600">
                        {formatDate(alloc.expiryDate)}
                      </td>
                      <td className="py-3.5 px-3 text-slate-600">
                        {alloc.remainingQuantity} {unit}
                      </td>
                      <td className="py-3.5 px-3 font-bold text-slate-900">
                        {alloc.quantity} {unit}
                      </td>
                      <td className="py-3.5 px-3 font-bold text-slate-900">{alloc.storageLocation || "-"}</td>
                      <td className="py-3.5 px-2 text-right">
                        <span className="inline-flex items-center rounded-full bg-emerald-50 px-2.5 py-0.5 text-[10px] font-semibold text-emerald-700 border border-emerald-200/60">
                          Active
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>

      {preview && preview.allocations.length > 0 && (
        <div className="mt-6 flex flex-col sm:flex-row sm:items-center justify-between rounded-xl border border-emerald-200/80 bg-emerald-50/50 p-4">
          <div>
            <span className="text-[11px] font-semibold uppercase tracking-wider text-slate-500">
              Total allocation
            </span>
            <p className="mt-0.5 text-[22px] font-bold text-emerald-700 leading-none">
              {preview.allocatedQuantity} {unit}
            </p>
          </div>

          <div className="mt-3 sm:mt-0 text-left sm:text-right">
            <span className="text-[11px] font-semibold uppercase tracking-wider text-slate-500">
              Preview time
            </span>
            <p className="mt-0.5 text-[12px] font-medium text-slate-700">
              {formatTime(preview.previewedAt)}
            </p>
          </div>
        </div>
      )}
    </div>
  );
};
