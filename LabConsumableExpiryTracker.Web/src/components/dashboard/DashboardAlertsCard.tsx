import { useMemo, useState, type FC } from "react";
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
import { DataTable, ItemBadge, type DataTableColumn } from "../DataTable";
import { EmptyState, ErrorState, LoadingState } from "../FeedBackStates";
import { useItems } from "../../hooks/useLot";
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
  if (diffDays < 0) return "Overdue";
  if (diffDays === 0) return "Today";
  return `${diffDays} days`;
};
const getBadgeInfo = (alertType: DashboardAlertType | string) => {
  const type = String(alertType).toLowerCase();
  if (type.includes("expiring"))
    return { label: "Expiring", className: "bg-[#fef9c3] text-[#b45309]" };
  if (type.includes("lowstock") || type.includes("low stock"))
    return { label: "Low stock", className: "bg-[#fef2f2] text-[#e11d48]" };
  if (type.includes("expired"))
    return { label: "Expired", className: "bg-[#fee2e2] text-[#b91c1c]" };
  if (type.includes("blocked"))
    return { label: "Blocked", className: "bg-[#f0fdfa] text-[#0f766e]" };
  return { label: String(alertType), className: "bg-slate-100 text-slate-600" };
};
export const DashboardAlertsCard: FC<DashboardAlertsCardProps> = ({
  onViewAll,
}) => {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(5);
  const { data: items } = useItems();
  const itemImageMap = useMemo(() => {
    const map = new Map<string, string | null>();
    items?.forEach((item) => {
      if (item.imageId) {
        map.set(item.id, item.imageId);
      }
    });
    return map;
  }, [items]);
  const columns: DataTableColumn<DashboardAlert>[] = [
    {
      id: "item",
      header: "Item / Lot",
      width: "35%",
      cell: (alert) => {
        const detailHref = getDetailHref(alert);
        return (
          <div className="flex items-center gap-3.5 pr-2">
            <Link
              to={detailHref}
              title={`View ${alert.resourceType === "Item" ? "item" : "lot"}`}
              aria-label={`View details of ${alert.itemName || alert.itemCode || "item"}`}
              className="shrink-0 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-teal-600 rounded-xl"
            >
              <ItemBadge
                fallbackText={alert.itemName || alert.itemCode || "I"}
                imageId={itemImageMap.get(alert.itemId) ?? null}
                tone="cyan"
                badgeSize="md"
                showStatusDot={false}
              />
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
        );
      },
    },
    {
      id: "sisa",
      header: "Remaining",
      cell: (alert) => (
        <span className="text-[13px] font-bold text-slate-800">
          {formatQuantity(alert.remainingQuantity, alert.baseUnit)}
        </span>
      ),
    },
    {
      id: "expiry",
      header: "Expiry",
      cell: (alert) =>
        alert.expiryDate ? (
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
        ),
    },
    {
      id: "status",
      header: "Status",
      cell: (alert) => {
        const badge = getBadgeInfo(alert.alertType);
        const actionHref = getActionHref(alert);
        return actionHref ? (
          <Link
            to={actionHref}
            title={getActionLabel(alert.alertType)}
            aria-label={`${badge.label}: ${getActionLabel(alert.alertType)}`}
            className={`inline-flex items-center justify-center rounded-full px-3.5 py-1 text-[11px] font-medium transition hover:opacity-80 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-teal-600 ${badge.className}`}
          >
            {badge.label}
          </Link>
        ) : (
          <span
            className={`inline-flex items-center justify-center rounded-full px-3.5 py-1 text-[11px] font-medium ${badge.className}`}
          >
            {badge.label}
          </span>
        );
      },
    },
  ];
  const { data, isLoading, isError, refetch } = useDashboardAlerts({
    page,
    pageSize,
  });
  const alerts: DashboardAlert[] = data?.items ?? [];
  const totalCount = data?.totalCount ?? 0;
  return (
    <section className="rounded-2xl border border-slate-200/90 bg-white p-6 sm:p-7 shadow-[0_1px_3px_rgb(15_23_42/0.03)]">
      <div className="flex items-start justify-between">
        <div>
          <h3 className="text-[20px] font-bold tracking-tight text-slate-800">
            Need Attention
          </h3>
          <p className="mt-1 text-[11px] text-slate-400">
            Prioritized based on expiry status and usable stock
          </p>
        </div>
        {onViewAll ? (
          <button
            type="button"
            onClick={onViewAll}
            className="rounded-lg border border-slate-200 bg-white px-4 py-1.5 text-[11px] font-semibold text-[#13a9a7] transition shadow-[0_1px_2px_rgb(15_23_42/0.02)] hover:bg-slate-50"
          >
            View All
          </button>
        ) : null}
      </div>
      <div className="mt-6">
        {isLoading ? (
          <LoadingState variant="table" rows={5} columns={4} />
        ) : isError ? (
          <ErrorState
            title="Failed to load alerts"
            description="An error occurred while retrieving alert data. Please try again."
            onRetry={() => void refetch()}
            retryLabel="Try Again"
          />
        ) : (
          <DataTable
            columns={columns}
            rows={alerts}
            getRowId={(alert) => `${alert.alertType}-${alert.resourceId}`}
            density="compact"
            emptyState={
              <EmptyState
                title="No Alerts"
                description="No inventory items require attention at this time."
              />
            }
            page={page}
            pageSize={pageSize}
            totalItems={totalCount}
            onPageChange={setPage}
            onPageSizeChange={(size) => {
              setPage(1);
              setPageSize(size);
            }}
          />
        )}
      </div>
    </section>
  );
};
