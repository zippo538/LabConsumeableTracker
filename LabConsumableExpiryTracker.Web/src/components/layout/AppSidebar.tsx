import {type FC} from "react";
import {NavLink, useNavigate} from "react-router";
import FormulatrixLogo from "../../assets/logo_formulatrix.png";
import {useLogout} from "../../hooks/useLogin";
import type {UserProfile} from "../../types/api";
import {
  LayoutDashboard,
  Package,
  Layers,
  History,
  Users,
  UserCircle,
  ClipboardList,
  LogOut,
  ChevronLeft,
  type LucideIcon,
} from "lucide-react";

const adminNav: ReadonlyArray<readonly [string, string, LucideIcon]> = [
  ["Dashboard", "/app/dashboard", LayoutDashboard],
  ["Item", "/app/items", Package],
  ["Lot", "/app/lots", Layers],
  ["Audit & History", "/app/audit", History],
  ["User Management", "/app/manage-users", Users],
  ["Profil", "/app/admin-profile", UserCircle],
];

const scientistNav: ReadonlyArray<readonly [string, string, LucideIcon]> = [
    ["Dashboard", "/app/dashboard", LayoutDashboard],
    ["Job", "/app/jobs", LayoutDashboard],
    ["Consume", "/app/consumption", ClipboardList],
    ["Consumption History", "/app/consumption-history", History],
    ["Profile", "/app/scientist-profile", UserCircle],
];


interface AppSidebarProps {
    isWarehouseAdmin: boolean;
    roleLabel: string;
    user: UserProfile | null;
    collapsed: boolean;
    onToggle: () => void;
}


export const AppSidebar: FC<AppSidebarProps> = ({
                                                    isWarehouseAdmin,
                                                    roleLabel,
                                                    user,
                                                    collapsed,
                                                    onToggle,
                                                }) => {
    const navigate = useNavigate();
    const {mutate: logout, isPending} = useLogout();

    const navItems = isWarehouseAdmin ? adminNav : scientistNav;

    const handleLogout = () =>
        logout(undefined, {
            onSettled: () => navigate("/login", {replace: true}),
        });

    const initials = (user?.fullName || user?.userName || "U")
        .split(" ")
        .map((w) => w[0])
        .join("")
        .slice(0, 2)
        .toUpperCase();

    return (
        <aside
            className={`
        fixed inset-y-0 left-0 z-20 hidden lg:flex flex-col
        bg-[#0c263b] text-slate-300
        transition-all duration-300 ease-in-out
        ${collapsed ? "w-[60px]" : "w-[200px]"}
      `}
        >

            <div
                className={`
          flex h-[58px] items-center shrink-0 overflow-hidden border-b border-white/5
          ${collapsed ? "justify-center px-0" : "px-4"}
        `}
            >
                {collapsed ? (
                    <span className="text-[#18b4b5] font-black text-lg tracking-tight select-none">
            L
          </span>
                ) : (
                    <img
                        src={FormulatrixLogo}
                        alt="Formulatrix"
                        className="h-5 w-auto brightness-0 invert"
                    />
                )}
            </div>


            <button
                type="button"
                onClick={onToggle}
                aria-label={collapsed ? "Perluas sidebar" : "Ciutkan sidebar"}
                className={`
          absolute top-[70px] -right-3 z-30
          flex h-6 w-6 items-center justify-center
          rounded-full bg-[#1a4a65] border border-[#2a6a8f]
          text-slate-300 hover:text-white hover:bg-[#1e5678]
          shadow-md transition-all duration-200
        `}
            >

                <ChevronLeft
                    strokeWidth={2.2}
                    className={`h-3 w-3 transition-transform duration-300 ${collapsed ? "rotate-180" : "rotate-0"}`}
                />
            </button>


            <nav
                className={`mt-4 flex flex-col gap-0.5 px-2 flex-1 overflow-y-auto overflow-x-hidden`}
                aria-label="Navigasi utama"
            >
                {navItems.map(([label, path, IconComp]) => (
                    <NavLink
                        key={label}
                        to={path}
                        title={collapsed ? label : undefined}
                        className={({isActive}) =>
                            `group relative flex items-center gap-3 rounded-lg px-2.5 py-2.5 text-[10px] font-medium transition-all duration-150 whitespace-nowrap overflow-hidden
              ${
                                isActive
                                    ? "bg-[#12465f] text-white shadow-sm"
                                    : "text-slate-400 hover:bg-white/5 hover:text-white"
                            }`
                        }
                    >
                        {({isActive}) => (
                            <>
                                <IconComp
                                    className={`h-3.5 w-3.5 shrink-0 transition-colors ${
                                        isActive
                                            ? "text-[#18b4b5]"
                                            : "text-slate-500 group-hover:text-slate-300"
                                    }`}
                                />


                                <span
                                    className={`transition-all duration-200 ${
                                        collapsed
                                            ? "opacity-0 w-0 pointer-events-none"
                                            : "opacity-100"
                                    }`}
                                >
                  {label}
                </span>


                                {isActive && (
                                    <span
                                        className="absolute left-0 top-1/2 -translate-y-1/2 h-4 w-0.5 rounded-r-full bg-[#18b4b5]"/>
                                )}
                            </>
                        )}
                    </NavLink>
                ))}
            </nav>


            <div
                className={`
          shrink-0 border-t border-white/5 p-2 pb-3
          transition-all duration-300
        `}
            >
                <div
                    className={`
            flex items-center rounded-xl bg-[#123652] p-2.5
            ${collapsed ? "justify-center" : "gap-2.5"}
          `}
                >
        
          <span
              className="grid h-7 w-7 shrink-0 place-items-center rounded-full bg-[#16b2b6] text-[8px] font-bold text-white">
            {initials}
          </span>


                    <div
                        className={`min-w-0 flex-1 transition-all duration-200 ${
                            collapsed ? "opacity-0 w-0 overflow-hidden" : "opacity-100"
                        }`}
                    >
                        <p className="truncate text-[9px] font-semibold text-white">
                            {user?.fullName || user?.userName || "Pengguna"}
                        </p>
                        <p className="mt-0.5 text-[8px] text-slate-400">{roleLabel}</p>

                        <button
                            type="button"
                            onClick={handleLogout}
                            disabled={isPending}
                            className="mt-2 flex items-center gap-1 text-[8px] text-slate-400 hover:text-rose-400 transition-colors disabled:cursor-not-allowed disabled:opacity-50"
                        >
                            <LogOut
                                strokeWidth={2}
                                className="h-2.5 w-2.5"
                            />
                            {isPending ? "Log out..." : "Log out"}
                        </button>
                    </div>

                    {collapsed && (
                        <button
                            type="button"
                            onClick={handleLogout}
                            disabled={isPending}
                            title="Log Out"
                            className="absolute bottom-4 left-1/2 -translate-x-1/2 text-slate-500 hover:text-rose-400 transition-colors disabled:opacity-50"
                        >
                            <LogOut
                                strokeWidth={2}
                                className="h-3.5 w-3.5"
                            />
                        </button>
                    )}
                </div>
            </div>
        </aside>
    );
};