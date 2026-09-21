import { useState } from "react";
import { useAtomValue } from "jotai";
import { AppSidebar } from "../components/layout/AppSidebar";
import { Icon } from "../components/ui/Icon";
import { userAtom } from "../lib/auth/authAtom";
import { ScientistDashboard } from "./ScientistDashboard";
import { WarehouseAdminDashboard } from "./WarehouseAdminDashboard";

export { Icon } from "../components/ui/Icon";

export const DashboardPage = () => {
  const user = useAtomValue(userAtom);
  const [active, setActive] = useState("Dashboard");
  const isWarehouseAdmin = user?.roles.includes("WarehouseAdmin") ?? false;
  const roleLabel = isWarehouseAdmin ? "Admin Gudang" : "Scientist";

  return (
    <div className="min-h-screen bg-[#f8fafc] text-slate-800">
      <AppSidebar
        active={active}
        isWarehouseAdmin={isWarehouseAdmin}
        onActiveChange={setActive}
        roleLabel={roleLabel}
        user={user}
      />

      <div className="lg:pl-[180px]">
        <header className="sticky top-0 z-10 flex h-[58px] items-center justify-between border-b border-slate-200 bg-white px-5 sm:px-7">
          <div>
            <h1 className="text-[15px] font-bold leading-tight text-slate-800">
              {active}
            </h1>
            <p className="mt-0.5 text-[8px] text-slate-400">
              {isWarehouseAdmin
                ? "Ringkasan stok gudang laboratorium"
                : "Ruang kerja laboratorium Anda"}
            </p>
          </div>
          <div className="flex items-center gap-2.5">
            <label className="hidden h-7 w-[195px] items-center gap-1.5 rounded-md border border-slate-200 px-2.5 sm:flex">
              <Icon name="search" className="h-3 w-3 text-slate-400" />
              <input
                aria-label="Cari item atau lot"
                placeholder="Cari item atau Lot..."
                className="w-full border-0 bg-transparent p-0 text-[8px] outline-none placeholder:text-slate-400"
              />
            </label>
            <button
              type="button"
              aria-label="Notifikasi"
              className="grid h-7 w-7 place-items-center rounded-md border border-slate-200 text-amber-500"
            >
              <span className="h-1 w-1 rounded-full bg-amber-500" />
            </button>
            <span className="rounded-full bg-[#eaf5ff] px-5 py-2 text-[8px] font-semibold text-[#1772a6]">
              {roleLabel}
            </span>
          </div>
        </header>

        <main className="mx-auto max-w-[1000px] px-5 py-6 sm:px-7">
          {isWarehouseAdmin ? (
            <WarehouseAdminDashboard user={user} />
          ) : (
            <ScientistDashboard user={user} />
          )}
        </main>
      </div>
    </div>
  );
};
