import type { FC } from "react";
import type { UserProfile } from "../types/api";
import { useDashboardSummary } from "../hooks/useDashboard";
import { DashboardAlertsCard } from "../components/dashboard/DashboardAlertsCard";
import { ExpiryStatusChart } from "../components/dashboard/ExpiryStatusChart";

interface DashboardProps {
  user: UserProfile | null;
}
const formatEvaluatedAt = (value: string) => {
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
          "Item low stock",
          "Perlu perhatian",
          "bg-rose-50 text-[#df2431]",
        ],
        [
          data.expiringSoonLotCount,
          "Mendekati expiry",
          "Lot masih eligible",
          "bg-amber-50 text-amber-500",
        ],
        [
          data.expiredLotCount,
          "Lot expired",
          "Tidak eligible",
          "bg-red-50 text-red-600",
        ],
        [
          data.blockedLotCount,
          "Lot blocked",
          "Termasuk quarantined",
          "bg-teal-50 text-[#13a9a7]",
        ],
      ] as const)
    : [];

  return (
    <div>
      <section className="flex items-start justify-between">
        <div>
          <h2 className="text-[20px] font-bold tracking-tight text-slate-800">
            Selamat datang,{" "}
            {user?.fullName?.split(" ")[0] || user?.userName || "Admin"}
          </h2>
          <p className="mt-1 text-[8px] text-slate-400">
            Ringkasan stok gudang berdasarkan evaluasi server.
          </p>
        </div>
        <button
          onClick={() => void refetch()}
          disabled={isFetching}
          className="flex items-center gap-1.5 rounded-md border border-slate-200 bg-white px-4 py-2 text-[8px] font-medium text-slate-600 hover:bg-slate-50 disabled:opacity-60"
        >
        
          {isFetching ? "Memperbarui..." : "Perbarui"}
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
          <p className="mt-2 text-[8px] text-slate-400">
            Terakhir dievaluasi server: {formatEvaluatedAt(data.evaluatedAt)}
          </p>
          <section className="mt-6 grid grid-cols-2 gap-3 lg:grid-cols-4">
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
                    <strong className="block text-[19px] leading-5">
                      {value}
                    </strong>
                    <span className="block text-[8px] font-semibold">
                      {title}
                    </span>
                  </div>
                </div>
                <p className="mt-4 text-[8px] text-slate-400">{note}</p>
              </article>
            ))}
          </section>

          <div className="mt-7">
            <DashboardAlertsCard />
          </div>

          <section className="mt-7 grid gap-5 lg:grid-cols-[1fr_220px]">
            <div className="rounded-xl border border-slate-200 bg-white p-5">
              <h3 className="text-[13px] font-bold">Ringkasan perhatian</h3>
              <p className="mt-1 text-[8px] leading-3.5 text-slate-400">
                Angka dashboard berasal dari{" "}
                <code className="rounded bg-slate-100 px-1 text-slate-500">
                  GET /api/dashboard/summary
                </code>
                . Daftar lot rinci memerlukan endpoint alerts yang masih
                berstatus Should di SRS.
              </p>
              <div className="mt-5 grid grid-cols-2 gap-3">
                <div className="rounded-lg bg-amber-50 p-3">
                  <p className="text-[8px] text-amber-700">Perlu ditinjau</p>
                  <strong className="mt-1 block text-xl text-amber-700">
                    {data.lowStockItemCount + data.expiringSoonLotCount}
                  </strong>
                </div>
                <div className="rounded-lg bg-red-50 p-3">
                  <p className="text-[8px] text-red-700">Tidak eligible</p>
                  <strong className="mt-1 block text-xl text-red-700">
                    {data.expiredLotCount + data.blockedLotCount}
                  </strong>
                </div>
              </div>
            </div>
            <aside className="space-y-5">
              <ExpiryStatusChart summary={data} />
              <div className="rounded-xl border border-[#b9e9ed] bg-[#effdff] p-4">
                <div className="flex items-center gap-3">
                  <span className="grid h-7 w-7 place-items-center rounded-lg bg-[#13aaad] text-white">
                    ✓
                  </span>
                  <p className="text-[9px] font-bold">
                    Kontrol eligibility aktif
                  </p>
                </div>
                <p className="mt-3 text-[8px] leading-3 text-slate-500">
                  Lot expired, quarantined, blocked, disposed, atau depleted
                  tidak masuk alokasi.
                </p>
                <span className="mt-3 inline-block rounded-full bg-[#d7f9f8] px-3 py-1 text-[7px] font-semibold text-teal-600">
                  FEFO siap digunakan
                </span>
              </div>
            </aside>
          </section>
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
    Memuat ringkasan dashboard…
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
