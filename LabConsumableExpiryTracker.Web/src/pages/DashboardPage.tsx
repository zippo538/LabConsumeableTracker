import { useState, type ReactNode } from "react";
import { useAtomValue } from "jotai";
import { useNavigate } from "react-router";
import { userAtom } from "../lib/auth/authAtom";
import { useLogout } from "../hooks/useLogin";
import { WarehouseAdminDashboard } from "./WarehouseAdminDashboard";
import { ScientistDashboard } from "./ScientistDashboard";
import FormulatrixLogo from "../assets/logo_formulatrix.png";

type IconName =
  | "dashboard"
  | "inventory"
  | "status"
  | "clipboard"
  | "history"
  | "users"
  | "profile"
  | "search"
  | "refresh"
  | "chevron";
export const Icon = ({
  name,
  className = "",
}: {
  name: IconName;
  className?: string;
}) => {
  const paths: Record<IconName, ReactNode> = {
    dashboard: (
      <>
        <rect x="3" y="3" width="7" height="7" rx="1" />
        <rect x="14" y="3" width="7" height="7" rx="1" />
        <rect x="3" y="14" width="7" height="7" rx="1" />
        <rect x="14" y="14" width="7" height="7" rx="1" />
      </>
    ),
    inventory: (
      <>
        <path d="M4 7.5 12 3l8 4.5-8 4.5-8-4.5Z" />
        <path d="M4 12.5 12 17l8-4.5M4 17 12 21l8-4" />
      </>
    ),
    status: (
      <>
        <path d="M12 3v18M3 12h18" />
        <circle cx="12" cy="12" r="8" />
      </>
    ),
    clipboard: (
      <>
        <rect x="5" y="4" width="14" height="17" rx="2" />
        <path d="M9 4.5V3h6v1.5M9 10h6M9 14h6" />
      </>
    ),
    history: (
      <>
        <path d="M3 12a9 9 0 1 0 3-6.7" />
        <path d="M3 4v5h5M12 7v5l3 2" />
      </>
    ),
    users: (
      <>
        <circle cx="9" cy="8" r="3" />
        <path d="M3.5 20c.6-3.3 2.5-5 5.5-5s4.9 1.7 5.5 5M17 7a3 3 0 0 1 0 5.8M18 15c1.4.7 2.3 2.1 2.6 4" />
      </>
    ),
    profile: (
      <>
        <circle cx="12" cy="8" r="3.5" />
        <path d="M5 21c.8-3.8 3.1-5.7 7-5.7s6.2 1.9 7 5.7" />
      </>
    ),
    search: (
      <>
        <circle cx="10.5" cy="10.5" r="5.5" />
        <path d="m15 15 4.5 4.5" />
      </>
    ),
    refresh: (
      <>
        <path d="M20 11a8 8 0 1 0 1.1 4" />
        <path d="M20 4v7h-7" />
      </>
    ),
    chevron: <path d="m9 18 6-6-6-6" />,
  };
  return (
    <svg
      className={className}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.8"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      {paths[name]}
    </svg>
  );
};
const adminNav = [
  ["Dashboard", "dashboard"],
  ["Item & Lot", "inventory"],
  ["Status Lot", "status"],
  ["Disposal", "clipboard"],
  ["Audit & Riwayat", "history"],
  ["Kelola Pengguna", "users"],
  ["Profil", "profile"],
] as const;
const scientistNav = [
  ["Dashboard", "dashboard"],
  ["Pilih Consumable", "inventory"],
  ["Konsumsi Saya", "clipboard"],
  ["Riwayat Penggunaan", "history"],
  ["Profil", "profile"],
] as const;
export const DashboardPage = () => {
  const user = useAtomValue(userAtom);
  const navigate = useNavigate();
  const { mutate: logout, isPending } = useLogout();
  const [active, setActive] = useState("Dashboard");
  const isWarehouseAdmin = ["warehouseadmin", "admin"].includes(
    user?.role?.toLowerCase() ?? "",
  );
  const roleLabel = isWarehouseAdmin ? "Admin Gudang" : "Scientist";
  const navItems = isWarehouseAdmin ? adminNav : scientistNav;
  const handleLogout = () =>
    logout(undefined, {
      onSettled: () => navigate("/login", { replace: true }),
    });
  return (
    <div className="min-h-screen bg-[#f8fafc] text-slate-800">
      <aside className="fixed inset-y-0 left-0 z-20 hidden w-[180px] flex-col bg-[#0c263b] px-3 py-6 text-slate-300 lg:flex">
        <img
          src={FormulatrixLogo}
          alt="Formulatrix"
          className="h-auto w-[148px] brightness-0 invert"
        />
        <nav className="mt-8 space-y-1.5">
          {navItems.map(([label, icon]) => (
            <button
              key={label}
              onClick={() => setActive(label)}
              className={`flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-left text-[10px] font-medium transition ${active === label ? "bg-[#12465f] text-white shadow-sm" : "hover:bg-white/5 hover:text-white"}`}
            >
              <Icon
                name={icon}
                className={`h-3.5 w-3.5 ${active === label ? "text-[#18b4b5]" : "text-slate-500"}`}
              />
              {label}
            </button>
          ))}
        </nav>
        <div className="mt-auto rounded-xl bg-[#123652] p-3">
          <div className="flex items-center gap-2.5">
            <span className="grid h-7 w-7 place-items-center rounded-full bg-[#16b2b6] text-[8px] font-bold text-white">
              {(user?.fullName || user?.userName || "U")
                .slice(0, 2)
                .toUpperCase()}
            </span>
            <div className="min-w-0">
              <p className="truncate text-[9px] font-semibold text-white">
                {user?.fullName || user?.userName || "Pengguna"}
              </p>
              <p className="mt-0.5 text-[8px] text-slate-400">{roleLabel}</p>
            </div>
          </div>
          <button
            onClick={handleLogout}
            disabled={isPending}
            className="mt-3 text-[9px] text-slate-400 hover:text-white"
          >
            {isPending ? "Keluar..." : "Keluar"}
          </button>
        </div>
      </aside>
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
