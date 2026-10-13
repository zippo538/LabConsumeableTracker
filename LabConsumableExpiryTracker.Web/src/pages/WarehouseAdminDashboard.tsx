import type { FC } from "react";
import type { UserProfile } from "../types/api";
import { useDashboardSummary } from "../hooks/useDashboard";
import { DashboardAlertsCard } from "../components/dashboard/DashboardAlertsCard";
import { ExpiryStatusChart } from "../components/dashboard/ExpiryStatusChart";
import { SummaryStatsCard } from "../components/SummaryStatsCard";

interface DashboardProps {
  user: UserProfile | null;
}
export const formatEvaluatedAt = (value: string) => {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return new Intl.DateTimeFormat("id-ID", {
    year: "numeric",
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
    timeZoneName: "short",
  }).format(date);
};

export const WarehouseAdminDashboard: FC<DashboardProps> = ({ user }) => {
  const { data, isLoading, isFetching, isError, error, refetch } =
    useDashboardSummary();

  return (
    <div>
      <section className="flex items-start justify-between">
        <div>
          <h2 className="text-2xl font-bold tracking-tight text-slate-800">
            Welcome,{" "}
            {user?.fullName?.split(" ")[0] || user?.userName || "Admin"}
          </h2>
          <p className="mt-1 text-xs text-slate-400">Summary Item Stock</p>
        </div>
        <button
          onClick={() => void refetch()}
          disabled={isFetching}
          className="flex items-center gap-1.5 rounded-md border border-slate-200 bg-white px-4 py-2 text-xs font-medium text-slate-600 hover:bg-slate-50 disabled:opacity-60"
        >
          {isFetching ? "Refresh..." : "Refresh"}
        </button>
      </section>
      {isLoading ? (
        <DashboardLoading />
      ) : isError || !data ? (
        <DashboardError
          message={error?.message ?? "Data dashboard tidak tersedia."}
          onRetry={() => void refetch()}
        />
      ) : (
        <>
          <p className="mt-2 text-xs text-slate-400 mb-5">
            Last Refresh : {formatEvaluatedAt(data.evaluatedAt)}
          </p>
          <SummaryStatsCard
            stats={
              [
                {
                  value: data.lowStockItemCount,
                  title: "Low Stock Items",
                  note: "Requires attention",
                  color: "rose",
                },
                {
                  value: data.expiringSoonLotCount,
                  title: "Expiring Soon Lots",
                  note: "Still eligible for consumption",
                  color: "amber",
                },
                {
                  value: data.expiredLotCount,
                  title: "Expired Lots",
                  note: "Not eligible for use",
                  color: "red",
                },
                {
                  value: data.blockedLotCount,
                  title: "Blocked Lots",
                  note: "Includes quarantined lots",
                  color: "teal",
                },
              ] as const
            }
          />

          <div className="mt-7 flex gap-6">
            <div className="flex-1 mt-5">
              <DashboardAlertsCard />
            </div>

            <div className="flex-1">
              <ExpiryStatusChart summary={data} />
            </div>
          </div>
        </>
      )}
    </div>
  );
};

export const DashboardLoading = () => (
  <div
    role="status"
    className="mt-7 rounded-xl border border-slate-200 bg-white p-8 text-center text-xs text-slate-500"
  >
    <span className="mx-auto mb-3 block h-5 w-5 animate-spin rounded-full border-2 border-teal-200 border-t-teal-600" />
    Loading dashboard summary...
  </div>
);
export const DashboardError = ({
  message,
  onRetry,
}: {
  message: string;
  onRetry: () => void;
}) => (
  <div
    role="alert"
    className="mt-7 rounded-xl border border-rose-200 bg-rose-50 p-5"
  >
    <p className="text-xs font-semibold text-rose-800">
      Failed Fetch Dashboard
    </p>
    <p className="mt-1 text-[10px] text-rose-700">{message}</p>
    <button
      onClick={onRetry}
      className="mt-3 rounded-md border border-rose-200 bg-white px-3 py-1.5 text-[9px] font-semibold text-rose-700"
    >
      Refresh
    </button>
  </div>
);
