import type { FC } from "react";
import type { UserProfile } from "../types/auth.types";
import { useDashboardSummary } from "../hooks/useDashboard";
import { DashboardError, DashboardLoading } from "./WarehouseAdminDashboard";
import { Icon } from "./DashboardPage";
import { DashboardAlertsCard } from "../components/dashboard/DashboardAlertsCard";
interface DashboardProps {
  user: UserProfile | null;
}
const actions = [
  [
    "Pilih consumable",
    "Gunakan lot yang direkomendasikan sistem berdasarkan FEFO.",
    "inventory",
  ],
  [
    "Catat konsumsi",
    "Rekam material yang digunakan untuk pekerjaan laboratorium.",
    "clipboard",
  ],
  [
    "Riwayat penggunaan",
    "Tinjau penggunaan bahan yang telah dicatat.",
    "history",
  ],
] as const;
export const ScientistDashboard: FC<DashboardProps> = ({ user }) => {
  const { data, isLoading, isFetching, isError, error, refetch } =
    useDashboardSummary();
  return (
    <div>
      <section className="flex items-start justify-between">
        <div>
          <h2 className="text-[20px] font-bold tracking-tight text-slate-800">
            Selamat datang,{" "}
            {user?.fullName?.split(" ")[0] || user?.userName || "Scientist"}
          </h2>
          <p className="mt-1 text-[8px] text-slate-400">
            Pilih lot eligible dan catat penggunaan pekerjaan laboratorium Anda.
          </p>
        </div>
        <button
          onClick={() => void refetch()}
          disabled={isFetching}
          className="flex items-center gap-1.5 rounded-md border border-slate-200 bg-white px-4 py-2 text-[8px] font-medium text-slate-600 disabled:opacity-60"
        >
          <Icon
            name="refresh"
            className={`h-2.5 w-2.5 ${isFetching ? "animate-spin" : ""}`}
          />
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
          <section className="mt-7 grid grid-cols-1 gap-3 sm:grid-cols-3">
            <article className="rounded-xl border border-teal-100 bg-[#f1fdfd] p-4">
              <p className="text-[9px] font-semibold text-teal-700">
                Lot mendekati expiry
              </p>
              <strong className="mt-2 block text-[23px] text-teal-700">
                {data.expiringSoonLotCount}
              </strong>
              <p className="mt-2 text-[8px] text-teal-600">
                Masih eligible; gunakan mengikuti urutan FEFO
              </p>
            </article>
            <article className="rounded-xl border border-rose-100 bg-white p-4">
              <p className="text-[9px] font-semibold text-slate-500">
                Item low stock
              </p>
              <strong className="mt-2 block text-[23px]">
                {data.lowStockItemCount}
              </strong>
              <p className="mt-2 text-[8px] text-slate-400">
                Ketersediaan stok perlu diperhatikan
              </p>
            </article>
            <article className="rounded-xl border border-red-100 bg-white p-4">
              <p className="text-[9px] font-semibold text-slate-500">
                Lot tidak eligible
              </p>
              <strong className="mt-2 block text-[23px]">
                {data.expiredLotCount + data.blockedLotCount}
              </strong>
              <p className="mt-2 text-[8px] text-slate-400">
                Expired atau blocked
              </p>
            </article>
          </section>

          <div className="mt-7">
            <DashboardAlertsCard />
          </div>

          <section className="mt-7">
            <h3 className="text-[13px] font-bold">Aksi cepat</h3>
            <p className="mt-1 text-[8px] text-slate-400">
              Semua penggunaan akan memprioritaskan lot dengan tanggal
              kedaluwarsa terdekat.
            </p>
            <div className="mt-4 grid gap-4 md:grid-cols-3">
              {actions.map(([title, description, icon]) => (
                <button
                  key={title}
                  className="group rounded-xl border border-slate-200 bg-white p-5 text-left transition hover:border-teal-300 hover:shadow-sm"
                >
                  <span className="grid h-9 w-9 place-items-center rounded-lg bg-[#e9fbfa] text-teal-600">
                    <Icon name={icon} className="h-4 w-4" />
                  </span>
                  <h4 className="mt-5 text-[12px] font-bold">{title}</h4>
                  <p className="mt-2 min-h-9 text-[8px] leading-3.5 text-slate-500">
                    {description}
                  </p>
                  <span className="mt-4 inline-flex items-center gap-1 text-[8px] font-semibold text-teal-600">
                    Mulai <Icon name="chevron" className="h-3 w-3" />
                  </span>
                </button>
              ))}
            </div>
          </section>
          <section className="mt-7 rounded-xl border border-[#b9e9ed] bg-[#effdff] p-5">
            <div className="flex items-start gap-3">
              <span className="grid h-8 w-8 shrink-0 place-items-center rounded-lg bg-[#13aaad] text-white">
                ✓
              </span>
              <div>
                <h3 className="text-[11px] font-bold">
                  Kontrol eligibility aktif
                </h3>
                <p className="mt-1 text-[8px] leading-3.5 text-slate-500">
                  Lot expired, quarantined, blocked, disposed, atau depleted
                  tidak dapat dipilih. Sistem menampilkan urutan FEFO untuk
                  membantu Anda memakai stok yang tepat terlebih dahulu.
                </p>
              </div>
            </div>
          </section>
        </>
      )}
    </div>
  );
};
