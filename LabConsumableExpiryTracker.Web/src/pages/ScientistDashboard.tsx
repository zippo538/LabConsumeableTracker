import type { FC } from "react";
import type { UserProfile } from "../types/api";
import { Icon } from "../components/ui/Icon";
import { useDashboardSummary } from "../hooks/useDashboard";
import { DashboardError, DashboardLoading } from "./WarehouseAdminDashboard";
import { DashboardAlertsCard } from "../components/dashboard/DashboardAlertsCard";
import { formatEvaluatedAt } from "../pages/WarehouseAdminDashboard";
import { SummaryStatsCard } from "../components/SummaryStatsCard";

interface DashboardProps {
  user: UserProfile | null;
}

export const ScientistDashboard: FC<DashboardProps> = ({ user }) => {
  const { data, isLoading, isFetching, isError, error, refetch } =
    useDashboardSummary();

  return (
    <div>
      <section className="flex items-start justify-between">
        <div>
          <h2 className="text-2xl font-bold tracking-tight text-slate-800">
            Welcome,{" "}
            {user?.fullName?.split(" ")[0] || user?.userName || "Scientist"}
          </h2>
          <p className="mt-1 text-xs text-slate-400">
            Select eligible lots and record your laboratory material usage.
          </p>
        </div>
        <button
          onClick={() => void refetch()}
          disabled={isFetching}
          className="flex items-center gap-1.5 rounded-md border border-slate-200 bg-white px-4 py-2 text-xs font-medium text-slate-600 disabled:opacity-60"
        >
          <Icon
            name="refresh"
            className={`h-2.5 w-2.5 ${isFetching ? "animate-spin" : ""}`}
          />
          {isFetching ? "Refresh..." : "Refresh"}
        </button>
      </section>
      {isLoading ? (
        <DashboardLoading />
      ) : isError || !data ? (
        <DashboardError
          message={error?.message ?? "Dashboard data is not available."}
          onRetry={() => void refetch()}
        />
      ) : (
        <>
          <p className="mt-2 text-xs text-slate-400">
            Last Refresh : {formatEvaluatedAt(data.evaluatedAt)}
          </p>
          <SummaryStatsCard
            className="grid-cols-1 sm:grid-cols-3 lg:grid-cols-3"
            stats={
              [
                {
                  value: data.expiringSoonLotCount,
                  title: "Expiring Soon Lots",
                  note: "Still eligible; use according to FEFO order",
                  color: "amber",
                },
                {
                  value: data.lowStockItemCount,
                  title: "Low Stock Items",
                  note: "Stock availability requires attention",
                  color: "rose",
                },
                {
                  value: data.expiredLotCount + data.blockedLotCount,
                  title: "Ineligible Lots",
                  note: "Expired or blocked lots cannot be used",
                  color: "red",
                },
              ] as const
            }
          />
          <div className="mt-7">
            <DashboardAlertsCard />
          </div>
        </>
      )}
    </div>
  );
};
