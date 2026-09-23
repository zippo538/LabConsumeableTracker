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
            <h1 className="text-xl font-bold leading-tight text-slate-800">
              {title}
            </h1>
            <p className="mt-0.5 text-xs text-slate-400">{description}</p>
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