// src/components/lots/LotTable.tsx
import { useMemo } from "react";
import type { Lot } from "../../types/lot.types";
import { LotStatusBadge } from "./LotStatusBadge";

interface LotTableProps {
  lots: Lot[];
  isLoading: boolean;
  onUpdate: (lot: Lot) => void;
  onDelete: (lot: Lot) => void;
  canEdit: boolean;
  // Pagination props
  page?: number;
  pageSize?: number;
  totalItems?: number;
  onPageChange?: (page: number) => void;
  onPageSizeChange?: (pageSize: number) => void;
}

export const LotTable = ({
  lots,
  isLoading,
  onUpdate,
  onDelete,
  canEdit,
  page = 1,
  pageSize = 20,
  totalItems = 0,
  onPageChange,
  onPageSizeChange,
}: LotTableProps) => {
  const totalPages = Math.max(1, Math.ceil(totalItems / pageSize));
  const firstItem = totalItems === 0 ? 0 : (page - 1) * pageSize + 1;
  const lastItem = Math.min(page * pageSize, totalItems);

  const visiblePages = useMemo(() => {
    const candidates = new Set([1, totalPages, page - 1, page, page + 1]);
    return [...candidates]
      .filter((candidate) => candidate >= 1 && candidate <= totalPages)
      .sort((a, b) => a - b);
  }, [page, totalPages]);

  const changePage = (nextPage: number) => {
    if (nextPage < 1 || nextPage > totalPages || nextPage === page) {
      return;
    }
    onPageChange?.(nextPage);
  };

  const changePageSize = (nextPageSize: number) => {
    onPageChange?.(1); // Reset ke halaman pertama saat mengubah pageSize
    onPageSizeChange?.(nextPageSize);
  };

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
    <div className="space-y-4">
      {/* Table */}
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
                    {lot.remainingQuantity} / {lot.initialQuantity}{" "}
                    {lot.baseUnit}
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
                  <p className="text-sm text-slate-800">
                    {lot.storageLocation}
                  </p>
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

      {/* Pagination */}
      {totalItems > 0 && (
        <div className="flex flex-col gap-4 border-t border-slate-200 pt-4 sm:flex-row sm:items-center sm:justify-between">
          {/* Info & Page Size */}
          <div className="flex flex-wrap items-center gap-3 text-xs text-slate-500">
            <span aria-live="polite">
              Menampilkan {firstItem}–{lastItem} dari {totalItems} lot
            </span>
            {onPageSizeChange && (
              <label className="flex items-center gap-2">
                <span>Baris per halaman</span>
                <select
                  value={pageSize}
                  onChange={(event) =>
                    changePageSize(Number(event.target.value))
                  }
                  aria-label="Jumlah lot per halaman"
                  className="h-8 rounded-lg border border-slate-200 bg-white px-2 text-xs font-semibold text-slate-700 outline-none focus:border-teal-400 focus:ring-2 focus:ring-teal-100"
                >
                  {[10, 20, 50, 100].map((size) => (
                    <option key={size} value={size}>
                      {size}
                    </option>
                  ))}
                </select>
              </label>
            )}
          </div>

          {/* Navigation */}
          {onPageChange && totalPages > 1 && (
            <nav
              aria-label="Pagination lot"
              className="flex items-center gap-1.5"
            >
              <button
                type="button"
                onClick={() => changePage(page - 1)}
                disabled={page <= 1}
                className="h-8 rounded-lg border border-slate-200 bg-white px-3 text-xs font-semibold text-slate-600 transition hover:border-teal-300 hover:text-teal-700 disabled:cursor-not-allowed disabled:opacity-40"
              >
                Sebelumnya
              </button>

              {visiblePages.map((pageNumber, index) => {
                const previousPage = visiblePages[index - 1];
                return (
                  <div key={pageNumber} className="flex items-center gap-1">
                    {previousPage && pageNumber - previousPage > 1 && (
                      <span className="px-1 text-slate-400">…</span>
                    )}
                    <button
                      type="button"
                      onClick={() => changePage(pageNumber)}
                      aria-label={`Buka halaman ${pageNumber}`}
                      aria-current={pageNumber === page ? "page" : undefined}
                      className={`grid h-8 min-w-8 place-items-center rounded-lg px-2 text-xs font-semibold transition ${
                        pageNumber === page
                          ? "bg-teal-600 text-white"
                          : "border border-slate-200 bg-white text-slate-600 hover:border-teal-300 hover:text-teal-700"
                      }`}
                    >
                      {pageNumber}
                    </button>
                  </div>
                );
              })}

              <button
                type="button"
                onClick={() => changePage(page + 1)}
                disabled={page >= totalPages}
                className="h-8 rounded-lg border border-slate-200 bg-white px-3 text-xs font-semibold text-slate-600 transition hover:border-teal-300 hover:text-teal-700 disabled:cursor-not-allowed disabled:opacity-40"
              >
                Berikutnya
              </button>
            </nav>
          )}
        </div>
      )}
    </div>
  );
};
