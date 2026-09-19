import type { ReactNode } from "react";

export type IconName =
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
