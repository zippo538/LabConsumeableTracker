import { type ReactNode, type CSSProperties, useMemo, useState } from "react";

export type TableAlign = "left" | "center" | "right";
export type TableDensity = "compact" | "default" | "comfortable";

export type DataTableColumn<T> = {
  id: string;
  header: string;
  cell: (row: T) => ReactNode;
  width?: string;
  minWidth?: string;
  align?: TableAlign;
  sortable?: boolean;
  sortValue?: (row: T) => string | number | Date | null | undefined;
  className?: string;
};

// ✅ Tambahkan props pagination
export type DataTableProps<T> = {
  columns: DataTableColumn<T>[];
  rows: T[];
  getRowId: (row: T) => string;
  density?: TableDensity;
  loading?: boolean;
  emptyState?: ReactNode;
  caption?: string;
  className?: string;
  getRowClassName?: (row: T) => string | undefined;
  // Pagination props
  page?: number;
  pageSize?: number;
  totalItems?: number;
  onPageChange?: (page: number) => void;
  onPageSizeChange?: (pageSize: number) => void;
};

type SortDirection = "asc" | "desc";
type SortState<T> = {
  column: DataTableColumn<T>;
  direction: SortDirection;
};

export function DataTable<T>({
  columns,
  rows,
  getRowId,
  density = "default",
  loading = false,
  emptyState = "No data found.",
  caption,
  className = "",
  getRowClassName,
  // ✅ Destructure pagination props
  page = 1,
  pageSize = 20,
  totalItems = 0,
  onPageChange,
  onPageSizeChange,
}: DataTableProps<T>) {
  const [sort, setSort] = useState<SortState<T> | null>(null);

  // Sorting logic (client-side for current page)
  const sortedRows = useMemo(() => {
    if (!sort?.column.sortValue) return rows;
    return [...rows].sort((a, b) => {
      const first = sort.column.sortValue?.(a);
      const second = sort.column.sortValue?.(b);
      const left = first instanceof Date ? first.getTime() : (first ?? "");
      const right = second instanceof Date ? second.getTime() : (second ?? "");
      const compared =
        typeof left === "number" && typeof right === "number"
          ? left - right
          : String(left).localeCompare(String(right));
      return sort.direction === "asc" ? compared : -compared;
    });
  }, [rows, sort]);

  function updateSort(column: DataTableColumn<T>) {
    if (!column.sortable || !column.sortValue) return;
    setSort((current) => ({
      column,
      direction:
        current?.column.id === column.id && current.direction === "asc"
          ? "desc"
          : "asc",
    }));
  }

  // ✅ Pagination calculations
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
    if (nextPage < 1 || nextPage > totalPages || nextPage === page) return;
    onPageChange?.(nextPage);
  };

  const changePageSize = (nextPageSize: number) => {
    onPageChange?.(1); // Reset ke halaman 1 saat ganti page size
    onPageSizeChange?.(nextPageSize);
  };

  return (
    <div
      className={`overflow-hidden rounded-xl border border-slate-200 bg-white ${className}`.trim()}
    >
      <div className="overflow-x-auto">
        <table className="w-full min-w-[720px] table-fixed border-collapse">
          {caption && <caption className="sr-only">{caption}</caption>}
          <colgroup>
            {columns.map((column) => (
              <col
                key={column.id}
                style={
                  {
                    width: column.width,
                    minWidth: column.minWidth,
                  } as CSSProperties
                }
              />
            ))}
          </colgroup>
          <thead>
            <tr>
              {columns.map((column) => {
                const isSorted = sort?.column.id === column.id;
                const ariaSort = isSorted
                  ? sort?.direction === "asc"
                    ? "ascending"
                    : "descending"
                  : "none";
                return (
                  <th
                    key={column.id}
                    aria-sort={column.sortable ? ariaSort : undefined}
                    className={`h-12 bg-slate-50 px-6 text-left text-[11px] font-bold uppercase tracking-[0.04em] text-slate-500 whitespace-nowrap ${alignClass[column.align ?? "left"]} ${column.className ?? ""}`}
                  >
                    {column.sortable && column.sortValue ? (
                      <button
                        type="button"
                        className="inline-flex cursor-pointer items-center gap-1.5 border-0 bg-transparent p-0 text-inherit"
                        onClick={() => updateSort(column)}
                      >
                        {column.header}
                        <span
                          aria-hidden="true"
                          className="text-sm tracking-normal text-slate-400"
                        >
                          {isSorted
                            ? sort?.direction === "asc"
                              ? "↑"
                              : "↓"
                            : "↕"}
                        </span>
                      </button>
                    ) : (
                      column.header
                    )}
                  </th>
                );
              })}
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <LoadingRows columnCount={columns.length} density={density} />
            ) : sortedRows.length > 0 ? (
              sortedRows.map((row) => (
                <tr key={getRowId(row)} className={getRowClassName?.(row)}>
                  {columns.map((column) => (
                    <td
                      key={column.id}
                      className={`${densityClass[density]} border-t border-slate-100 px-6 align-middle text-sm text-slate-800 ${alignClass[column.align ?? "left"]} ${column.className ?? ""}`}
                    >
                      {column.cell(row)}
                    </td>
                  ))}
                </tr>
              ))
            ) : (
              <tr>
                <td
                  className="border-t border-slate-100 px-6 py-12 text-center text-slate-500"
                  colSpan={columns.length}
                >
                  {emptyState}
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {/* ✅ Pagination UI Footer */}
      {totalItems > 0 && onPageChange && (
        <div className="flex flex-col gap-4 border-t border-slate-200 px-6 py-4 sm:flex-row sm:items-center sm:justify-between">
          <div className="flex flex-wrap items-center gap-3 text-[11px] text-slate-500">
            <span aria-live="polite">
              Menampilkan {firstItem}–{lastItem} dari {totalItems} data
            </span>
            {onPageSizeChange && (
              <label className="flex items-center gap-2">
                <span>Baris per halaman</span>
                <select
                  value={pageSize}
                  onChange={(event) =>
                    changePageSize(Number(event.target.value))
                  }
                  className="h-8 rounded-lg border border-slate-200 bg-white px-2 text-[11px] font-semibold text-slate-700 outline-none focus:border-teal-400 focus:ring-2 focus:ring-teal-100"
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

          {totalPages > 1 && (
            <nav aria-label="Pagination" className="flex items-center gap-1.5">
              <button
                type="button"
                onClick={() => changePage(page - 1)}
                disabled={page <= 1}
                className="h-8 rounded-lg border border-slate-200 bg-white px-3 text-[11px] font-semibold text-slate-600 transition hover:border-teal-300 hover:text-teal-700 disabled:cursor-not-allowed disabled:opacity-40"
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
                      aria-current={pageNumber === page ? "page" : undefined}
                      className={`grid h-8 min-w-8 place-items-center rounded-lg px-2 text-[11px] font-semibold transition ${
                        pageNumber === page
                          ? "bg-[#13a9a7] text-white"
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
                className="h-8 rounded-lg border border-slate-200 bg-white px-3 text-[11px] font-semibold text-slate-600 transition hover:border-teal-300 hover:text-teal-700 disabled:cursor-not-allowed disabled:opacity-40"
              >
                Berikutnya
              </button>
            </nav>
          )}
        </div>
      )}
    </div>
  );
}

function LoadingRows({
  columnCount,
  density,
}: {
  columnCount: number;
  density: TableDensity;
}) {
  const rowCount = density === "compact" ? 5 : 4;

  return Array.from({ length: rowCount }, (_, rowIndex) => (
    <tr key={rowIndex} aria-label="Loading row">
      {Array.from({ length: columnCount }, (_, columnIndex) => (
        <td
          key={columnIndex}
          className={`${densityClass[density]} border-t border-slate-100 px-6 align-middle`}
        >
          <span
            className="block h-3.5 animate-pulse rounded-full bg-slate-100"
            style={{ width: `${55 + ((rowIndex + columnIndex) % 3) * 15}%` }}
          />
        </td>
      ))}
    </tr>
  ));
}

export function TableToolbar({ children }: { children: ReactNode }) {
  return (
    <div className="mb-6 flex flex-wrap items-center gap-3">{children}</div>
  );
}

export function EntityCell({
  title,
  subtitle,
  imageUrl,
  fallback,
  imageAlt = "",
}: {
  title: string;
  subtitle?: string;
  imageUrl?: string | null;
  fallback?: ReactNode;
  imageAlt?: string;
}) {
  const [imageFailed, setImageFailed] = useState(false);

  return (
    <div className="flex min-w-0 items-center gap-3">
      <div
        className="grid h-10 w-10 shrink-0 place-items-center overflow-hidden rounded-lg bg-cyan-50 text-[13px] font-bold text-teal-700"
        aria-hidden={!imageAlt}
      >
        {imageUrl && !imageFailed ? (
          <img
            className="h-full w-full object-cover"
            src={imageUrl}
            alt={imageAlt}
            onError={() => setImageFailed(true)}
          />
        ) : (
          (fallback ?? <span>{title.slice(0, 1).toUpperCase()}</span>)
        )}
      </div>
      <div className="grid min-w-0 gap-0.5">
        <span className="truncate font-semibold text-slate-900">{title}</span>
        {subtitle && (
          <span className="truncate text-xs text-slate-500">{subtitle}</span>
        )}
      </div>
    </div>
  );
}

export type StatusTone = "neutral" | "success" | "warning" | "danger" | "info";

export function StatusBadge({
  label,
  tone = "neutral",
}: {
  label: string;
  tone?: StatusTone;
}) {
  return (
    <span
      className={`inline-flex min-h-7 items-center whitespace-nowrap rounded-full px-3 text-xs font-semibold ${statusToneClass[tone]}`}
    >
      {label}
    </span>
  );
}

export function RowAction({
  children = "•••",
  label = "Open row actions",
  onClick,
}: {
  children?: ReactNode;
  label?: string;
  onClick?: () => void;
}) {
  return (
    <button
      type="button"
      className="h-[34px] min-w-9 rounded-lg border border-slate-200 bg-white font-bold text-slate-600 hover:border-teal-500 hover:text-teal-700 focus:outline-none focus-visible:ring-2 focus-visible:ring-teal-500 focus-visible:ring-offset-2"
      aria-label={label}
      onClick={onClick}
    >
      {children}
    </button>
  );
}

export function TablePagination({
  page,
  totalPages,
  onPageChange,
}: {
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}) {
  const pages = Array.from({ length: totalPages }, (_, index) => index + 1);

  return (
    <nav className="mt-4 flex justify-end gap-1" aria-label="Pagination">
      <button
        type="button"
        className="h-9 w-9 rounded-lg border border-slate-200 bg-white text-slate-600 disabled:cursor-not-allowed disabled:opacity-45"
        disabled={page === 1}
        onClick={() => onPageChange(page - 1)}
      >
        ‹
      </button>
      {pages.map((item) => (
        <button
          key={item}
          type="button"
          className={`h-9 w-9 rounded-lg border text-slate-600 ${item === page ? "border-teal-500 bg-teal-500 text-white" : "border-slate-200 bg-white"}`}
          aria-current={item === page ? "page" : undefined}
          onClick={() => onPageChange(item)}
        >
          {item}
        </button>
      ))}
      <button
        type="button"
        className="h-9 w-9 rounded-lg border border-slate-200 bg-white text-slate-600 disabled:cursor-not-allowed disabled:opacity-45"
        disabled={page === totalPages}
        onClick={() => onPageChange(page + 1)}
      >
        ›
      </button>
    </nav>
  );
}

const alignClass: Record<TableAlign, string> = {
  left: "text-left",
  center: "text-center",
  right: "text-right",
};

const densityClass: Record<TableDensity, string> = {
  compact: "py-2.5",
  default: "py-[15px]",
  comfortable: "py-5",
};

const statusToneClass: Record<StatusTone, string> = {
  neutral: "bg-slate-100 text-slate-600",
  success: "bg-green-100 text-green-700",
  warning: "bg-amber-100 text-amber-700",
  danger: "bg-red-100 text-red-600",
  info: "bg-sky-100 text-sky-700",
};
