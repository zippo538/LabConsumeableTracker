import type { FC } from "react";
import type { UserProfile } from "../types/api";
import { useDashboardSummary } from "../hooks/useDashboard";
import { DashboardAlertsCard } from "../components/dashboard/DashboardAlertsCard";
import { ExpiryStatusChart } from "../components/dashboard/ExpiryStatusChart";
import { Alert } from "../components/MessageAlert";

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
  const stats = data
    ? ([
        [
          data.lowStockItemCount,
          "Low Stock Items",
          "Requires attention",
          "bg-rose-50 text-[#df2431]",
        ],
        [
          data.expiringSoonLotCount,
          "Expiring Soon Lots",
          "Still eligible for consumption",
          "bg-amber-50 text-amber-500",
        ],
        [
          data.expiredLotCount,
          "Expired Lots",
          "Not eligible for use",
          "bg-red-50 text-red-600",
        ],
        [
          data.blockedLotCount,
          "Blocked Lots",
          "Includes quarantined lots",
          "bg-teal-50 text-[#13a9a7]",
        ],
      ] as const)
    : [];

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
          <p className="mt-2 text-xs text-slate-400">
            Last Refresh : {formatEvaluatedAt(data.evaluatedAt)}
          </p>
          <section className="mt-6 grid grid-cols-2 gap-3 lg:grid-cols-4 ">
            {stats.map(([value, title, note, tone]) => (
              <article
                key={title}
                className="rounded-xl border border-slate-200 bg-white p-3 shadow-[0_1px_2px_rgb(15_23_42/0.02)]"
              >
                <div className="flex gap-3">
                  <span
                    className={`grid h-7 w-7 place-items-center rounded-lg ${tone}`}
                  >
                    <span className="h-2.5 w-2.5 rounded-[3px] bg-current" />
                  </span>
                  <div>
                    <strong className="text-2xl font-bold text-slate-900 leading-5">
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

          <div className="mt-7 flex gap-6">
            <div className="flex-1">
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
      Ringkasan dashboard gagal dimuat
    </p>
    <p className="mt-1 text-[10px] text-rose-700">{message}</p>
    <button
      onClick={onRetry}
      className="mt-3 rounded-md border border-rose-200 bg-white px-3 py-1.5 text-[9px] font-semibold text-rose-700"
    >
      Coba lagi
    </button>
  </div>
);
