import { useEffect, useMemo, useState, type FC, type ReactNode } from "react";
import { Link } from "react-router";
import { useDashboardAlerts } from "../../hooks/useDashboard";
import type {
  DashboardAlert,
  DashboardAlertType,
} from "../../types/dashboard.types";
import {
  getActionHref,
  getActionLabel,
  getDetailHref,
} from "../../helpers/dashboardAlertActions";

interface DashboardAlertsCardProps {
  onViewAll?: () => void;
}

const formatUnit = (unit?: string | number | null): string => {
  if (unit === null || unit === undefined) return "";
  const u = String(unit).toLowerCase();
  if (u === "1" || u === "milliliter") return "mL";
  if (u === "2" || u === "gram") return "g";
  if (u === "3" || u === "unit") return "unit";
  if (u === "4" || u === "vial") return "vial";
  return String(unit);
};

const formatQuantity = (
  qty?: number | null,
  unit?: string | number | null,
): string => {
  const formattedQty = qty ?? 0;
  const u = formatUnit(unit);
  return u ? `${formattedQty} ${u}` : `${formattedQty}`;
};

const formatExpiryDate = (dateStr: string): string => {
  const d = new Date(dateStr);
  if (Number.isNaN(d.getTime())) return dateStr;
  return new Intl.DateTimeFormat("id-ID", {
    day: "numeric",
    month: "short",
    year: "numeric",
  }).format(d);
};

const formatRelativeExpiry = (dateStr: string): string => {
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const target = new Date(dateStr);
  target.setHours(0, 0, 0, 0);
  if (Number.isNaN(target.getTime())) return "";
  const diffMs = target.getTime() - today.getTime();
  const diffDays = Math.round(diffMs / (1000 * 60 * 60 * 24));
  if (diffDays < 0) return "Lewat";
  if (diffDays === 0) return "Hari ini";
  return `${diffDays} hari`;
};

const getBadgeInfo = (alertType: DashboardAlertType | string) => {
  const type = String(alertType).toLowerCase();
  if (type.includes("expiring")) {
    return {
      label: "Expiring",
      className: "bg-[#fef9c3] text-[#b45309]",
    };
  }
  if (type.includes("lowstock") || type.includes("low stock")) {
    return {
      label: "Low stock",
      className: "bg-[#fef2f2] text-[#e11d48]",
    };
  }
  if (type.includes("expired")) {
    return {
      label: "Expired",
      className: "bg-[#fee2e2] text-[#b91c1c]",
    };
  }
  if (type.includes("blocked")) {
    return {
      label: "Blocked",
      className: "bg-[#f0fdfa] text-[#0f766e]",
    };
  }
  return {
    label: String(alertType),
    className: "bg-slate-100 text-slate-600",
  };
};

export const DashboardAlertsCard: FC<DashboardAlertsCardProps> = ({
  onViewAll,
}) => {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(2);

  const { data, isLoading, isFetching, isError, error, refetch } =
    useDashboardAlerts({ page, pageSize });

  const alerts: DashboardAlert[] = data?.items ?? [];
  const totalCount = data?.totalCount ?? 0;
  const responsePage = data?.page ?? page;
  const responsePageSize = data?.pageSize ?? pageSize;
  const totalPages = Math.max(1, Math.ceil(totalCount / responsePageSize));
  const firstItem =
    totalCount === 0 ? 0 : (responsePage - 1) * responsePageSize + 1;
  const lastItem = Math.min(responsePage * responsePageSize, totalCount);

  const visiblePages = useMemo(() => {
    const candidates = new Set([
      1,
      totalPages,
      responsePage - 1,
      responsePage,
      responsePage + 1,
    ]);

    return [...candidates]
      .filter((candidate) => candidate >= 1 && candidate <= totalPages)
      .sort((a, b) => a - b);
  }, [responsePage, totalPages]);

  useEffect(() => {
    if (page > totalPages) setPage(totalPages);
  }, [page, totalPages]);

  const changePage = (nextPage: number) => {
    if (nextPage < 1 || nextPage > totalPages || nextPage === responsePage) {
      return;
    }
    setPage(nextPage);
  };

  const changePageSize = (nextPageSize: number) => {
    setPage(1);
    setPageSize(nextPageSize);
  };

  const renderPageButtons = () => {
    const buttons: ReactNode[] = [];

    visiblePages.forEach((pageNumber, index) => {
      const previousPage = visiblePages[index - 1];
      if (previousPage && pageNumber - previousPage > 1) {
        buttons.push(
          <span key={`ellipsis-${pageNumber}`} className="px-1 text-slate-400">
            …
          </span>,
        );
      }

      buttons.push(
        <button
          key={pageNumber}
          type="button"
          onClick={() => changePage(pageNumber)}
          disabled={isFetching}
          aria-label={`Buka halaman ${pageNumber}`}
          aria-current={pageNumber === responsePage ? "page" : undefined}
          className={`grid h-8 min-w-8 place-items-center rounded-lg px-2 text-[11px] font-semibold transition disabled:cursor-not-allowed disabled:opacity-60 ${
            pageNumber === responsePage
              ? "bg-[#13a9a7] text-white"
              : "border border-slate-200 bg-white text-slate-600 hover:border-teal-300 hover:text-teal-700"
          }`}
        >
          {pageNumber}
        </button>,
      );
    });

    return buttons;
  };

  return (
    <section className="rounded-2xl border border-slate-200/90 bg-white p-6 sm:p-7 shadow-[0_1px_3px_rgb(15_23_42/0.03)]">
      <div className="flex items-start justify-between">
        <div>
          <h3 className="text-[20px] font-bold tracking-tight text-slate-800">
            Perlu perhatian
          </h3>
          <p className="mt-1 text-[11px] text-slate-400">
            Prioritas berdasarkan expiry dan stok usable
          </p>
        </div>
        {onViewAll ? (
          <button
            type="button"
            onClick={onViewAll}
            className="rounded-lg border border-slate-200 bg-white px-4 py-1.5 text-[11px] font-semibold text-[#13a9a7] transition shadow-[0_1px_2px_rgb(15_23_42/0.02)] hover:bg-slate-50"
          >
            Lihat semua
          </button>
        ) : null}
      </div>

      <div className="mt-6 overflow-x-auto">
        <div className="min-w-[540px]">
          <div className="grid grid-cols-[minmax(180px,2fr)_1fr_1.2fr_1fr] items-center rounded-lg bg-[#f8fafc] px-5 py-3 text-[10px] font-bold tracking-wider text-slate-400 uppercase">
            <div>ITEM / LOT</div>
            <div>SISA</div>
            <div>EXPIRY</div>
            <div>STATUS</div>
          </div>

          {isLoading ? (
            <div className="py-10 text-center text-xs text-slate-400">
              <span className="mx-auto mb-2 block h-4 w-4 animate-spin rounded-full border-2 border-teal-200 border-t-teal-600" />
              Memuat data alert...
            </div>
          ) : isError ? (
            <div className="py-8 text-center text-xs text-rose-600">
              <p>Gagal memuat alert: {error?.message}</p>
              <button
                type="button"
                onClick={() => void refetch()}
                className="mt-2 text-[10px] font-semibold text-teal-600 underline"
              >
                Coba lagi
              </button>
            </div>
          ) : alerts.length === 0 ? (
            <div className="py-8 text-center text-xs text-slate-400">
              Tidak ada inventaris yang memerlukan perhatian saat ini.
            </div>
          ) : (
            <div className="divide-y divide-slate-100">
              {alerts.map((alert) => {
                const badge = getBadgeInfo(alert.alertType);
                const detailHref = getDetailHref(alert);
                const actionHref = getActionHref(alert);
                const isExpired = String(alert.alertType)
                  .toLowerCase()
                  .includes("expired");
                return (
                  <div
                    key={`${alert.alertType}-${alert.resourceId}`}
                    className="grid grid-cols-[minmax(180px,2fr)_1fr_1.2fr_1fr] items-center px-5 py-4 transition hover:bg-slate-50/60"
                  >
                    {/* Item / Lot */}
                    <div className="flex items-center gap-3.5 pr-2">
                      <Link
                        to={detailHref}
                        title={`Lihat detail ${alert.resourceType === "Item" ? "item" : "lot"}`}
                        aria-label={`Lihat detail ${alert.itemName || alert.itemCode || "item"}`}
                        className="grid h-10 w-10 shrink-0 place-items-center rounded-xl bg-[#e6f8f8] text-[#13a9a7] transition hover:bg-[#d4f1f1] focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-teal-600"
                      >
                        <svg
                          className="h-4 w-4"
                          viewBox="0 0 16 16"
                          fill="currentColor"
                          aria-hidden="true"
                        >
                          <rect x="2" y="2" width="3" height="3" rx="0.75" />
                          <rect x="6.5" y="2" width="3" height="3" rx="0.75" />
                          <rect x="11" y="2" width="3" height="3" rx="0.75" />
                          <rect x="2" y="6.5" width="3" height="3" rx="0.75" />
                          <rect
                            x="6.5"
                            y="6.5"
                            width="3"
                            height="3"
                            rx="0.75"
                          />
                          <rect x="11" y="6.5" width="3" height="3" rx="0.75" />
                          <rect x="2" y="11" width="3" height="3" rx="0.75" />
                          <rect x="6.5" y="11" width="3" height="3" rx="0.75" />
                          <rect x="11" y="11" width="3" height="3" rx="0.75" />
                        </svg>
                      </Link>
                      <div className="min-w-0">
                        <p className="truncate text-[13px] font-bold text-slate-800">
                          {alert.itemName || alert.itemCode || "Item"}
                        </p>
                        <p className="truncate text-[11px] text-slate-400 mt-0.5">
                          {alert.lotNumber || alert.itemCode || "-"}
                        </p>
                      </div>
                    </div>

                    {/* Sisa */}
                    <div>
                      <span className="text-[13px] font-bold text-slate-800">
                        {formatQuantity(
                          alert.remainingQuantity,
                          alert.baseUnit,
                        )}
                      </span>
                    </div>

                    {/* Expiry */}
                    <div>
                      {alert.expiryDate ? (
                        <>
                          <p className="text-[13px] font-bold text-slate-800">
                            {formatExpiryDate(alert.expiryDate)}
                          </p>
                          <p className="text-[11px] text-slate-400 mt-0.5">
                            {formatRelativeExpiry(alert.expiryDate)}
                          </p>
                        </>
                      ) : (
                        <span className="text-[12px] text-slate-400">-</span>
                      )}
                    </div>

                    {/* Status */}
                    <div>
                      <div className="flex flex-col items-start gap-1.5">
                        <span
                          className={`inline-flex items-center justify-center rounded-full px-3.5 py-1 text-[11px] font-medium ${badge.className}`}
                        >
                          {badge.label}
                        </span>
                        {isExpired && actionHref ? (
                          <Link
                            to={actionHref}
                            title={getActionLabel(alert.alertType)}
                            className="text-[11px] font-semibold text-teal-700 transition hover:text-teal-900 hover:underline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-teal-600"
                          >
                            {getActionLabel(alert.alertType)}
                          </Link>
                        ) : null}
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>
      </div>

      {!isLoading && !isError && totalCount > 0 ? (
        <div className="mt-5 flex flex-col gap-4 border-t border-slate-100 pt-5 sm:flex-row sm:items-center sm:justify-between">
          <div className="flex flex-wrap items-center gap-3 text-[11px] text-slate-500">
            <span aria-live="polite">
              Menampilkan {firstItem}–{lastItem} dari {totalCount} alert
            </span>
            <label className="flex items-center gap-2">
              <span>Baris per halaman</span>
              <select
                value={pageSize}
                onChange={(event) => changePageSize(Number(event.target.value))}
                disabled={isFetching}
                aria-label="Jumlah alert per halaman"
                className="h-8 rounded-lg border border-slate-200 bg-white px-2 text-[11px] font-semibold text-slate-700 outline-none focus:border-teal-400 focus:ring-2 focus:ring-teal-100 disabled:cursor-not-allowed disabled:opacity-60"
              >
                {[2, 5, 10, 20].map((size) => (
                  <option key={size} value={size}>
                    {size}
                  </option>
                ))}
              </select>
            </label>
          </div>

          <nav
            aria-label="Pagination alert dashboard"
            className="flex items-center gap-1.5"
          >
            <button
              type="button"
              onClick={() => changePage(responsePage - 1)}
              disabled={responsePage <= 1 || isFetching}
              className="h-8 rounded-lg border border-slate-200 bg-white px-3 text-[11px] font-semibold text-slate-600 transition hover:border-teal-300 hover:text-teal-700 disabled:cursor-not-allowed disabled:opacity-40"
            >
              Sebelumnya
            </button>
            {renderPageButtons()}
            <button
              type="button"
              onClick={() => changePage(responsePage + 1)}
              disabled={responsePage >= totalPages || isFetching}
              className="h-8 rounded-lg border border-slate-200 bg-white px-3 text-[11px] font-semibold text-slate-600 transition hover:border-teal-300 hover:text-teal-700 disabled:cursor-not-allowed disabled:opacity-40"
            >
              Berikutnya
            </button>
          </nav>
        </div>
      ) : null}
    </section>
  );
};
