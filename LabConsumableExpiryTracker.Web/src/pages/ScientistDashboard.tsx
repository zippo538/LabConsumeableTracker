import type { FC } from "react";
import type { UserProfile } from "../types/api";
import { Icon } from "../components/ui/Icon";
import { useDashboardSummary } from "../hooks/useDashboard";
import { DashboardError, DashboardLoading } from "./WarehouseAdminDashboard";
import { DashboardAlertsCard } from "../components/dashboard/DashboardAlertsCard";
import {formatEvaluatedAt} from "../pages/WarehouseAdminDashboard"
interface DashboardProps {
  user: UserProfile | null;
}

export const ScientistDashboard: FC<DashboardProps> = ({ user }) => {
  const { data, isLoading, isFetching, isError, error, refetch } =
    useDashboardSummary();
  const stats = data
    ? ([
        [
          data.expiringSoonLotCount,
          "Expiring Soon Lots",
          "Still eligible; use according to FEFO order",
          "bg-amber-50 text-amber-500",
        ],
        [
          data.lowStockItemCount,
          "Low Stock Items",
          "Stock availability requires attention",
          "bg-rose-50 text-[#df2431]",
        ],
        [
          data.expiredLotCount + data.blockedLotCount,
          "Ineligible Lots",
          "Expired or blocked lots cannot be used",
          "bg-red-50 text-red-600",
        ],
      ] as const)
    : [];
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
          <section className="mt-6 grid grid-cols-1 gap-3 sm:grid-cols-3">
            {stats.map(([value, title, note, tone]) => (
              <article
                key={title}
                className="rounded-xl border border-slate-200 bg-white p-4 shadow-[0_1px_2px_rgb(15_23_42/0.02)]"
              >
                <div className="flex items-center gap-3">
                  <span
                    className={`grid h-8 w-8 shrink-0 place-items-center rounded-lg ${tone}`}
                  >
                    <span className="h-2.5 w-2.5 rounded-[3px] bg-current" />
                  </span>

                  <div>
                    <strong className="text-2xl font-bold leading-6 text-slate-900">
                      {value}
                    </strong>

                    <span className="block text-xs font-medium text-slate-500">
                      {title}
                    </span>
                  </div>
                </div>

                <p className="mt-4 text-xs text-slate-400">{note}</p>
              </article>
            ))}
          </section>
          <div className="mt-7">
            <DashboardAlertsCard />
          </div>
        </>
      )}
    </div>
  );
};
