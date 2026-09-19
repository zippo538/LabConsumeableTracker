import type { ReactNode } from "react";
import { useAuth } from "../../features/auth/AuthContext";
import type { AppRole } from "../../types/api";

type NavItem = { href: string; label: string; roles?: AppRole[] };

const navigation: NavItem[] = [
  { href: "/app/dashboard", label: "Dashboard" },
  { href: "/app/items", label: "Master Item" },
  { href: "/app/lots", label: "Lot" }, // ✅ TAMBAHAN: Navigasi ke halaman Lot
  { href: "/app/disposal", label: "Disposal", roles: ["WarehouseAdmin"] },
  { href: "/app/audit", label: "Audit & Riwayat" },
  { href: "/app/users", label: "Kelola Pengguna", roles: ["WarehouseAdmin"] },
  { href: "/app/profile", label: "Profil" },
];

function navigate(href: string): void {
  window.history.pushState({}, "", href);
  window.dispatchEvent(new Event("app:navigate"));
}

export function AppLink({
  href,
  children,
  className,
}: {
  href: string;
  children: ReactNode;
  className?: string;
}) {
  return (
    <a
      className={className}
      href={href}
      onClick={(event) => {
        if (event.metaKey || event.ctrlKey || event.shiftKey || event.altKey)
          return;
        event.preventDefault();
        navigate(href);
      }}
    >
      {children}
    </a>
  );
}

export function AppShell({
  children,
  path,
}: {
  children: ReactNode;
  path: string;
}) {
  const { session, logout } = useAuth();
  if (!session) return null;

  const isAllowed = (roles?: AppRole[]) =>
    !roles || roles.some((role) => session.user.roles.includes(role));

  return (
    <div className="app-shell">
      <header className="topbar">
        <AppLink href="/app/dashboard" className="brand">
          LabTrack <span>Expiry</span>
        </AppLink>
        <div className="account-menu">
          <div>
            <strong>{session.user.fullName}</strong>
            <small>{session.user.roles.join(", ")}</small>
          </div>
          <button
            type="button"
            className="button button-quiet"
            onClick={() => {
              logout();
              navigate("/login");
            }}
          >
            Logout
          </button>
        </div>
      </header>

      <nav className="sidebar">
        <ul>
          {navigation.map((item) => {
            if (!isAllowed(item.roles)) return null;

            const isActive =
              path === item.href || path.startsWith(`${item.href}/`);

            return (
              <li key={item.href}>
                <AppLink href={item.href} className={isActive ? "active" : ""}>
                  {item.label}
                </AppLink>
              </li>
            );
          })}
        </ul>
      </nav>

      <main className="main-content">{children}</main>
    </div>
  );
}
