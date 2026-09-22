import type {
  ExpiryCondition,
  LotAdministrativeStatus,
} from "../../types/lot.types";

interface LotStatusBadgeProps {
  status: ExpiryCondition | LotAdministrativeStatus;
}

export const LotStatusBadge = ({ status }: LotStatusBadgeProps) => {
  const config: Record<string, { label: string; className: string }> = {
    Valid: {
      label: "Valid",
      className: "bg-green-100 text-green-700",
    },
    ExpiringSoon: {
      label: "Expiring Soon",
      className: "bg-orange-100 text-orange-700",
    },
    Expired: {
      label: "Expired",
      className: "bg-red-100 text-red-700",
    },
    Active: {
      label: "Active",
      className: "bg-emerald-100 text-emerald-800",
    },
    Quarantined: {
      label: "Quarantined",
      className: "bg-amber-100 text-amber-800",
    },
    ManuallyBlocked: {
      label: "Blocked",
      className: "bg-rose-100 text-rose-800",
    },
    Disposed: {
      label: "Disposed",
      className: "bg-slate-100 text-slate-600",
    },
  };

  const badge = config[status] || {
    label: status,
    className: "bg-slate-100 text-slate-700",
  };

  return (
    <span
      className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${badge.className}`}
    >
      {badge.label}
    </span>
  );
};