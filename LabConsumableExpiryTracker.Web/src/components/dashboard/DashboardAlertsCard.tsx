import { useState, type FC } from "react";
import { useDashboardAlerts } from "../../hooks/useDashboard";
import type {
  DashboardAlert,
  DashboardAlertType,
} from "../../types/dashboard.types";

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
  const [isExpanded, setIsExpanded] = useState(false);

  const { data, isLoading, isError, error, refetch } = useDashboardAlerts({
    page: 1,
    pageSize: isExpanded ? 20 : 5,
  });

  const alerts: DashboardAlert[] = data?.items ?? [];

  const handleViewAllClick = () => {
    if (onViewAll) {
      onViewAll();
    } else {
      setIsExpanded((prev) => !prev);
    }
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
        <button
          type="button"
          onClick={handleViewAllClick}
          className="rounded-lg border border-slate-200 bg-white px-4 py-1.5 text-[11px] font-semibold text-[#13a9a7] hover:bg-slate-50 transition shadow-[0_1px_2px_rgb(15_23_42/0.02)]"
        >
          {isExpanded ? "Tampilkan lebih sedikit" : "Lihat semua"}
        </button>
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
              {alerts.map((alert, idx) => {
                const badge = getBadgeInfo(alert.alertType);
                return (
                  <div
                    key={`${alert.resourceId}-${idx}`}
                    className="grid grid-cols-[minmax(180px,2fr)_1fr_1.2fr_1fr] items-center px-5 py-4 transition hover:bg-slate-50/60"
                  >
                    {/* Item / Lot */}
                    <div className="flex items-center gap-3.5 pr-2">
                      <div className="grid h-10 w-10 shrink-0 place-items-center rounded-xl bg-[#e6f8f8] text-[#13a9a7]">
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
                      </div>
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
                      <span
                        className={`inline-flex items-center justify-center rounded-full px-3.5 py-1 text-[11px] font-medium ${badge.className}`}
                      >
                        {badge.label}
                      </span>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>
      </div>
    </section>
  );
};
