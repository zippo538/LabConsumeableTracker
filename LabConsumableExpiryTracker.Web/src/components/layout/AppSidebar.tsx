import type { FC } from "react";
import { useNavigate } from "react-router";
import FormulatrixLogo from "../../assets/logo_formulatrix.png";
import { useLogout } from "../../hooks/useLogin";
import type { UserProfile } from "../../types/api";
import { Icon, type IconName } from "../ui/Icon";

const adminNav: ReadonlyArray<readonly [string, IconName]> = [
  ["Dashboard", "dashboard"],
  ["Item & Lot", "inventory"],
  ["Status Lot", "status"],
  ["Disposal", "clipboard"],
  ["Audit & Riwayat", "history"],
  ["Kelola Pengguna", "users"],
  ["Profil", "profile"],
];
const scientistNav: ReadonlyArray<readonly [string, IconName]> = [
  ["Dashboard", "dashboard"],
  ["Pilih Consumable", "inventory"],
  ["Konsumsi Saya", "clipboard"],
  ["Riwayat Penggunaan", "history"],
  ["Profil", "profile"],
];

interface AppSidebarProps {
  active: string;
  isWarehouseAdmin: boolean;
  onActiveChange: (label: string) => void;
  roleLabel: string;
  user: UserProfile | null;
}

export const AppSidebar: FC<AppSidebarProps> = ({
  active,
  isWarehouseAdmin,
  onActiveChange,
  roleLabel,
  user,
}) => {
  const navigate = useNavigate();
  const { mutate: logout, isPending } = useLogout();
  const navItems = isWarehouseAdmin ? adminNav : scientistNav;
  const handleLogout = () =>
    logout(undefined, {
      onSettled: () => navigate("/login", { replace: true }),
    });

  return (
    <aside className="fixed inset-y-0 left-0 z-20 hidden w-[180px] flex-col bg-[#0c263b] px-3 py-6 text-slate-300 lg:flex">
      <img
        src={FormulatrixLogo}
        alt="Formulatrix"
        className="h-auto w-[148px] brightness-0 invert"
      />
      <nav className="mt-8 space-y-1.5" aria-label="Navigasi utama">
        {navItems.map(([label, icon]) => (
          <button
            key={label}
            type="button"
            onClick={() => onActiveChange(label)}
            aria-current={active === label ? "page" : undefined}
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
          type="button"
          onClick={handleLogout}
          disabled={isPending}
          className="mt-3 text-[9px] text-slate-400 hover:text-white disabled:cursor-not-allowed disabled:opacity-60"
        >
          {isPending ? "Keluar..." : "Keluar"}
        </button>
      </div>
    </aside>
  );
};
