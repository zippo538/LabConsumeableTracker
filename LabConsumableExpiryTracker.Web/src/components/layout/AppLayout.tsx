import { useState, type ReactNode } from "react";
import { useAtomValue } from "jotai";
import { AppSidebar } from "./AppSidebar";
import { Icon } from "../ui/Icon";
import { userAtom } from "../../lib/auth/authAtom";

type AppLayoutProps = {
  children: ReactNode;
  title: string;
  description: string;
};

export const AppLayout = ({ children, title, description }: AppLayoutProps) => {
  const user = useAtomValue(userAtom);
  const [collapsed, setCollapsed] = useState(false);

  const isWarehouseAdmin = user?.roles?.includes("WarehouseAdmin") ?? false;
  const roleLabel = isWarehouseAdmin ? "Admin Gudang" : "Scientist";

  return (
    <div className="min-h-screen bg-[#f8fafc] text-slate-800">
      {/* Sidebar */}
      <AppSidebar
        isWarehouseAdmin={isWarehouseAdmin}
        roleLabel={roleLabel}
        user={user}
        collapsed={collapsed}
        onToggle={() => setCollapsed((c) => !c)}
      />

      {/* Main area — slides right based on sidebar width */}
      <div
        className={`
          flex flex-col min-h-screen
          transition-all duration-300 ease-in-out
          ${collapsed ? "lg:pl-[60px]" : "lg:pl-[200px]"}
        `}
      >
        {/* Top navbar */}
        <header className="sticky top-0 z-10 flex h-[58px] shrink-0 items-center justify-between border-b border-slate-200 bg-white px-5 sm:px-7">
          <div>
            <h1 className="text-[15px] font-bold leading-tight text-slate-800">
              {title}
            </h1>
            <p className="mt-0.5 text-[8px] text-slate-400">{description}</p>
          </div>

          <div className="flex items-center gap-2.5">
            {/* Search box */}
            <label className="hidden h-7 w-[195px] items-center gap-1.5 rounded-md border border-slate-200 px-2.5 sm:flex">
              <Icon name="search" className="h-3 w-3 text-slate-400" />
              <input
                aria-label="Cari item atau lot"
                placeholder="Cari item atau Lot..."
                className="w-full border-0 bg-transparent p-0 text-[8px] outline-none placeholder:text-slate-400"
              />
            </label>

            {/* Notification dot */}
            <button
              type="button"
              aria-label="Notifikasi"
              className="grid h-7 w-7 place-items-center rounded-md border border-slate-200 text-amber-500"
            >
              <span className="h-1 w-1 rounded-full bg-amber-500" />
            </button>

            {/* Role badge */}
            <span className="rounded-full bg-[#eaf5ff] px-3 py-1.5 text-[8px] font-semibold text-[#1772a6]">
              {roleLabel}
            </span>
          </div>
        </header>

        {/* Page content */}
        <main className="mx-auto w-full max-w-[1100px] flex-1 px-5 py-6 sm:px-7">
          {children}
        </main>
      </div>
    </div>
  );
};